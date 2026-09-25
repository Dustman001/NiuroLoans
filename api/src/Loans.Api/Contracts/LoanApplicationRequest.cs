using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Loans.Application.LoanApplications;
using Loans.Domain;

namespace Loans.Api.Contracts;

/// <summary>
/// The form as posted. Runs the domain's own validation to collect every field error at once,
/// so the UI can show them together, then builds the command.
/// </summary>
public sealed record LoanApplicationRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? AddressLine1,
    string? AddressLine2,
    int? StateId,
    string? ZipCode,
    string? CompanyName,
    decimal? RequestedAmount,
    string? Ssn)
{
    public bool TryCreateCommand(
        [NotNullWhen(true)] out SubmitLoanApplication? command,
        out Dictionary<string, string[]> errors)
    {
        var fieldErrors = new Dictionary<string, string>(ApplicantDetails.Validate(
            FirstName, LastName, Email, AddressLine1, AddressLine2, StateId, ZipCode, CompanyName));

        if (LoanApplication.ValidateRequestedAmount(RequestedAmount) is { } amountError)
        {
            fieldErrors[ToFieldName(nameof(RequestedAmount))] = amountError;
        }

        if (!Domain.Ssn.TryParse(Ssn, out var ssn))
        {
            fieldErrors[ToFieldName(nameof(Ssn))] = Domain.Ssn.FormatErrorMessage;
        }

        errors = fieldErrors.ToDictionary(error => error.Key, error => new[] { error.Value });
        if (errors.Count > 0 || ssn is null)
        {
            command = null;
            return false;
        }

        var applicant = new ApplicantDetails(
            FirstName!, LastName!, Email!, AddressLine1!, AddressLine2, StateId!.Value, ZipCode!, CompanyName!);
        command = new SubmitLoanApplication(ssn, applicant, RequestedAmount!.Value);
        return true;
    }

    /// <summary>Error keys match the JSON field names the form sends (camelCase).</summary>
    private static string ToFieldName(string propertyName) => JsonNamingPolicy.CamelCase.ConvertName(propertyName);
}
