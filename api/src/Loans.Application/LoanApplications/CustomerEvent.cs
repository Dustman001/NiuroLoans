using Loans.Domain;

namespace Loans.Application.LoanApplications;

/// <summary>
/// The JSON contract sent to the events service. Plain fields, so renaming a domain property
/// cannot silently change what the external service receives.
/// </summary>
public sealed record CustomerEvent(
    CustomerEventType Type,
    string Ssn,
    string FirstName,
    string LastName,
    string Email,
    string AddressLine1,
    string? AddressLine2,
    int StateId,
    string ZipCode,
    string CompanyName,
    decimal RequestedAmount,
    DateTime SubmittedAtUtc)
{
    public static CustomerEvent From(CustomerEventType type, Customer customer, LoanApplication loanApplication)
    {
        var details = customer.Details;
        return new CustomerEvent(
            type,
            customer.Ssn.Value,
            details.FirstName,
            details.LastName,
            details.Email,
            details.AddressLine1,
            details.AddressLine2,
            details.StateId,
            details.ZipCode,
            details.CompanyName,
            loanApplication.RequestedAmount,
            loanApplication.SubmittedAtUtc);
    }
}
