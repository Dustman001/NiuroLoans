namespace Loans.Application.LoanApplications.Rules;

public sealed class LoanRuleEngine(IEnumerable<ILoanRule> rules)
{
    /// <returns>The first denial reason, or <c>null</c> when every rule passes.</returns>
    public async Task<DenialReason?> EvaluateAsync(SubmitLoanApplication command, CancellationToken cancellationToken)
    {
        foreach (var rule in rules)
        {
            var denialReason = await rule.EvaluateAsync(command, cancellationToken);
            if (denialReason is not null)
            {
                return denialReason;
            }
        }

        return null;
    }
}
