using Loans.Application.LoanApplications;

namespace Loans.Api.Contracts;

/// <summary>The decision. The UI owns the wording shown for each denial reason.</summary>
public sealed record LoanApplicationResponse(bool Approved, DenialReason? Reason);
