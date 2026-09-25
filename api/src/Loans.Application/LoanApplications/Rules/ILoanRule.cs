namespace Loans.Application.LoanApplications.Rules;

/// <summary>One approval rule. Rules run in registration order; the first denial wins.</summary>
public interface ILoanRule
{
    /// <returns>The reason to deny the application, or <c>null</c> when the rule passes.</returns>
    Task<DenialReason?> EvaluateAsync(SubmitLoanApplication command, CancellationToken cancellationToken);
}
