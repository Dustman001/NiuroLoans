namespace Loans.Application.LoanApplications;

/// <summary>The outcome of a loan application: approved, or denied with a reason.</summary>
public sealed record LoanDecision
{
    public static readonly LoanDecision Approval = new(denialReason: null);

    private LoanDecision(DenialReason? denialReason) => DenialReason = denialReason;

    public DenialReason? DenialReason { get; }

    public bool IsApproved => DenialReason is null;

    public static LoanDecision Denial(DenialReason reason) => new(reason);
}
