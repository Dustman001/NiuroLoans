using Loans.Domain;

namespace Loans.Application.LoanApplications;

/// <summary>The use-case input: a validated loan application as submitted by the form.</summary>
public sealed record SubmitLoanApplication(Ssn Ssn, ApplicantDetails Applicant, decimal RequestedAmount);
