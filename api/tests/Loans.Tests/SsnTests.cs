using Loans.Domain;

namespace Loans.Tests;

public sealed class SsnTests
{
    [Theory]
    [InlineData("123-45-6789", true)]
    [InlineData("123456789", true)]
    [InlineData(" 123-45-6789 ", true)]
    [InlineData("1-2345678-9", false)]
    [InlineData("12-345-6789", false)]
    [InlineData("123-456789", false)]
    [InlineData("12345-6789", false)]
    [InlineData("١٢٣-٤٥-٦٧٨٩", false)]
    public void Accepts_only_the_standard_format(string input, bool expected)
    {
        Assert.Equal(expected, Ssn.TryParse(input, out _));
    }

    [Fact]
    public void Is_stored_as_digits_and_shown_with_dashes()
    {
        var ssn = Ssn.Parse("123456789");

        Assert.Equal("123456789", ssn.Value);
        Assert.Equal("123-45-6789", ssn.ToString());
    }
}
