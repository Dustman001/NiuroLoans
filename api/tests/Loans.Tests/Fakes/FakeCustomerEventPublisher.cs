using Loans.Application.Abstractions;
using Loans.Application.LoanApplications;

namespace Loans.Tests.Fakes;

public sealed class FakeCustomerEventPublisher : ICustomerEventPublisher
{
    public List<CustomerEvent> Published { get; } = [];
    public int Attempts { get; private set; }

    /// <summary>How many of the next calls should fail before delivery succeeds.</summary>
    public int FailuresBeforeSuccess { get; set; }

    /// <summary>The exception a failing call throws.</summary>
    public Func<Exception> Failure { get; set; } = () => new HttpRequestException("Events service unavailable.");

    public Task PublishAsync(CustomerEvent customerEvent, CancellationToken cancellationToken)
    {
        Attempts++;
        if (FailuresBeforeSuccess > 0)
        {
            FailuresBeforeSuccess--;
            throw Failure();
        }

        Published.Add(customerEvent);
        return Task.CompletedTask;
    }
}
