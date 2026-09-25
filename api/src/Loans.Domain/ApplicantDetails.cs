using System.Net.Mail;

namespace Loans.Domain;

/// <summary>
/// The personal data an applicant provides. A value object: it is always valid once constructed,
/// and it is replaced as a whole when a returning customer submits new data.
/// </summary>
public sealed record ApplicantDetails
{
    public const int NameMaxLength = 100;
    public const int TextMaxLength = 200;
    public const int ZipCodeLength = 5;

    public ApplicantDetails(
        string firstName,
        string lastName,
        string email,
        string addressLine1,
        string? addressLine2,
        int stateId,
        string zipCode,
        string companyName)
    {
        var errors = Validate(firstName, lastName, email, addressLine1, addressLine2, stateId, zipCode, companyName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors.Values));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
        StateId = stateId;
        ZipCode = zipCode.Trim();
        CompanyName = companyName.Trim();
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public int StateId { get; }
    public string ZipCode { get; }
    public string CompanyName { get; }

    /// <summary>
    /// The single source of the applicant rules. Returns one message per invalid field,
    /// keyed by parameter name (which matches the form's JSON field names).
    /// </summary>
    public static IReadOnlyDictionary<string, string> Validate(
        string? firstName,
        string? lastName,
        string? email,
        string? addressLine1,
        string? addressLine2,
        int? stateId,
        string? zipCode,
        string? companyName)
    {
        var errors = new Dictionary<string, string>();
        CheckText(errors, nameof(firstName), firstName, NameMaxLength, required: true);
        CheckText(errors, nameof(lastName), lastName, NameMaxLength, required: true);
        CheckText(errors, nameof(addressLine1), addressLine1, TextMaxLength, required: true);
        CheckText(errors, nameof(addressLine2), addressLine2, TextMaxLength, required: false);
        CheckText(errors, nameof(companyName), companyName, TextMaxLength, required: true);

        if (!IsValidEmail(email))
        {
            errors[nameof(email)] = "Enter a valid email address.";
        }

        if (stateId is null or <= 0)
        {
            errors[nameof(stateId)] = "Select a state.";
        }

        if (zipCode?.Trim() is not { Length: ZipCodeLength } zip || !zip.All(char.IsAsciiDigit))
        {
            errors[nameof(zipCode)] = $"Zip code must have {ZipCodeLength} digits.";
        }

        return errors;
    }

    private static bool IsValidEmail(string? email)
    {
        var trimmed = email?.Trim();
        return !string.IsNullOrEmpty(trimmed)
            && trimmed.Length <= TextMaxLength
            && MailAddress.TryCreate(trimmed, out var address)
            && address.Address == trimmed;
    }

    private static void CheckText(Dictionary<string, string> errors, string field, string? value, int maxLength, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                errors[field] = "This field is required.";
            }
        }
        else if (value.Trim().Length > maxLength)
        {
            errors[field] = $"Use at most {maxLength} characters.";
        }
    }
}
