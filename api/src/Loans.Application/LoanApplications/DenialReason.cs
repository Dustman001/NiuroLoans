namespace Loans.Application.LoanApplications;

public enum DenialReason
{
    /// <summary>The applicant's state is not allowed.</summary>
    State,

    /// <summary>The applicant's SSN is blacklisted.</summary>
    Ssn,

    /// <summary>Saving or publishing failed after every retry.</summary>
    Unavailable,
}
