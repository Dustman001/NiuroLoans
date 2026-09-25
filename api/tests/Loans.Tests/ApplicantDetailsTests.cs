using Loans.Domain;

namespace Loans.Tests;

public sealed class ApplicantDetailsTests
{
    [Fact]
    public void Trims_values_and_drops_an_empty_second_address_line()
    {
        var details = new ApplicantDetails(" Ana ", "Lopez", " ana@example.com ", "1 Market St", "  ", 1, " 94105 ", "Acme");

        Assert.Equal("Ana", details.FirstName);
        Assert.Equal("ana@example.com", details.Email);
        Assert.Equal("94105", details.ZipCode);
        Assert.Null(details.AddressLine2);
    }

    [Theory]
    [InlineData("", "ana@example.com", "94105")]
    [InlineData("Ana", "not-an-email", "94105")]
    [InlineData("Ana", "Ana <ana@example.com>", "94105")]
    [InlineData("Ana", "ana@example.com", "9410")]
    public void Rejects_invalid_data(string firstName, string email, string zipCode)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new ApplicantDetails(firstName, "Lopez", email, "1 Market St", null, 1, zipCode, "Acme"));
    }

    [Fact]
    public void Rejects_text_longer_than_its_database_column()
    {
        var tooLong = new string('a', ApplicantDetails.NameMaxLength + 1);

        var errors = ApplicantDetails.Validate(tooLong, "Lopez", "ana@example.com", "1 Market St", null, 1, "94105", "Acme");

        Assert.Equal("Use at most 100 characters.", errors["firstName"]);
    }

    [Fact]
    public void Validate_reports_every_invalid_field_by_its_form_name()
    {
        var errors = ApplicantDetails.Validate(null, null, "bad", null, null, null, "1", null);

        Assert.Equal(
            new[] { "addressLine1", "companyName", "email", "firstName", "lastName", "stateId", "zipCode" },
            errors.Keys.Order(StringComparer.Ordinal));
    }
}
