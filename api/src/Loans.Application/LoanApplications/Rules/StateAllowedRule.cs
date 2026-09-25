using Loans.Application.Abstractions;

namespace Loans.Application.LoanApplications.Rules;

public sealed class StateAllowedRule(IStateRepository states) : ILoanRule
{
    public async Task<DenialReason?> EvaluateAsync(SubmitLoanApplication command, CancellationToken cancellationToken)
    {
        var state = await states.FindAsync(command.Applicant.StateId, cancellationToken);
        return state is null || state.IsNotAllowed ? DenialReason.State : null;
    }
}
