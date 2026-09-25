using Loans.Domain;

namespace Loans.Tests;

public sealed class LoanApplicationTests
{
    [Theory]
    [InlineData("0.01")]
    [InlineData("2.50")]
    [InlineData("9999999999999999.99")]
    public void Accepts_a_positive_amount_that_fits_the_column(string amount)
    {
        Assert.Null(LoanApplication.ValidateRequestedAmount(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Theory]
    [InlineData("0", "Requested amount must be greater than 0.")]
    [InlineData("-1", "Requested amount must be greater than 0.")]
    [InlineData("1.005", "Use at most 2 decimal places.")]
    [InlineData("10000000000000000.00", "Requested amount is too large.")]
    public void Rejects_an_amount_the_column_cannot_hold(string amount, string expectedError)
    {
        Assert.Equal(expectedError, LoanApplication.ValidateRequestedAmount(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Rejects_a_missing_amount()
    {
        Assert.Equal("Requested amount must be greater than 0.", LoanApplication.ValidateRequestedAmount(null));
    }
}
