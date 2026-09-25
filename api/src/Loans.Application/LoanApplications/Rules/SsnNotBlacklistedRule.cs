using Loans.Application.Abstractions;

namespace Loans.Application.LoanApplications.Rules;

public sealed class SsnNotBlacklistedRule(IBlacklistRepository blacklist) : ILoanRule
{
    public async Task<DenialReason?> EvaluateAsync(SubmitLoanApplication command, CancellationToken cancellationToken)
    {
        var entry = await blacklist.FindAsync(command.Ssn, cancellationToken);
        return entry is null ? null : DenialReason.Ssn;
    }
}
