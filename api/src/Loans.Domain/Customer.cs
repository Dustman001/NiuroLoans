namespace Loans.Domain;

/// <summary>A person who applied for a loan, identified by SSN.</summary>
public sealed class Customer
{
    public int Id { get; private set; }
    public Ssn Ssn { get; private set; } = null!;
    public ApplicantDetails Details { get; private set; } = null!;

    private Customer() { }

    public static Customer Register(Ssn ssn, ApplicantDetails details) => new() { Ssn = ssn, Details = details };

    public void UpdateDetails(ApplicantDetails details) => Details = details;
}
