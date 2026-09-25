using Loans.Application.Abstractions;
using Loans.Application.LoanApplications.Rules;
using Loans.Domain;
using Microsoft.Extensions.Logging;

namespace Loans.Application.LoanApplications;

public sealed class LoanApplicationService(
    LoanRuleEngine ruleEngine,
    ICustomerRepository customers,
    ILoanApplicationRepository loanApplications,
    ICustomerEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<LoanApplicationService> logger)
{
    public const int MaxAttempts = 3;

    public async Task<LoanDecision> SubmitAsync(SubmitLoanApplication command, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                // The rules read the database too, so they are retried with the rest of the unit.
                var denialReason = await ruleEngine.EvaluateAsync(command, cancellationToken);
                if (denialReason is not null)
                {
                    return LoanDecision.Denial(denialReason.Value);
                }

                await unitOfWork.ExecuteInTransactionAsync(ct => SaveAndPublishAsync(command, ct), cancellationToken);
                return LoanDecision.Approval;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                // Includes timeouts (TaskCanceledException): only a cancelled request stops the retries.
                logger.LogWarning(exception, "Submitting the application failed (attempt {Attempt}/{MaxAttempts}).", attempt, MaxAttempts);
            }
        }

        return LoanDecision.Denial(DenialReason.Unavailable);
    }

    private async Task SaveAndPublishAsync(SubmitLoanApplication command, CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var customer = await customers.FindBySsnAsync(command.Ssn, cancellationToken);
        var eventType = customer is null ? CustomerEventType.NewCustomer : CustomerEventType.ReturningCustomer;

        if (customer is null)
        {
            customer = Customer.Register(command.Ssn, command.Applicant);
            customers.Add(customer);
        }
        else
        {
            customer.UpdateDetails(command.Applicant);
        }

        var loanApplication = eventType == CustomerEventType.ReturningCustomer
            ? await loanApplications.FindByCustomerIdAsync(customer.Id, cancellationToken)
            : null;

        if (loanApplication is null)
        {
            loanApplication = LoanApplication.Submit(customer, command.RequestedAmount, nowUtc);
            loanApplications.Add(loanApplication);
        }
        else
        {
            loanApplication.Resubmit(command.RequestedAmount, nowUtc);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Published before the transaction commits: if delivery fails, the whole unit rolls back.
        await eventPublisher.PublishAsync(CustomerEvent.From(eventType, customer, loanApplication), cancellationToken);
    }
}
