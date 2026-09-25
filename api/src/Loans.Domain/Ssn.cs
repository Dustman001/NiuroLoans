using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Loans.Domain;

/// <summary>A US Social Security Number, stored as 9 digits and displayed as XXX-XX-XXXX.</summary>
public sealed partial record Ssn
{
    public const string FormatErrorMessage = "SSN must have the format XXX-XX-XXXX.";

    public string Value { get; }

    private Ssn(string value) => Value = value;

    /// <summary>Accepts <c>XXX-XX-XXXX</c> or 9 digits without dashes.</summary>
    public static bool TryParse(string? input, [NotNullWhen(true)] out Ssn? ssn)
    {
        var trimmed = input?.Trim() ?? string.Empty;
        ssn = SsnPattern().IsMatch(trimmed) ? new Ssn(trimmed.Replace("-", string.Empty)) : null;
        return ssn is not null;
    }

    public static Ssn Parse(string input) =>
        TryParse(input, out var ssn) ? ssn : throw new FormatException(FormatErrorMessage);

    public override string ToString() => $"{Value[..3]}-{Value[3..5]}-{Value[5..]}";

    // [0-9], not \d: in .NET \d also matches non-ASCII digits.
    [GeneratedRegex(@"^([0-9]{3}-[0-9]{2}-[0-9]{4}|[0-9]{9})$")]
    private static partial Regex SsnPattern();
}
