namespace Loans.Domain;

/// <summary>A customer's loan application. The spec keeps one per customer: resubmitting updates it.</summary>
public sealed class LoanApplication
{
    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateTime SubmittedAtUtc { get; private set; }
    public decimal RequestedAmount { get; private set; }

    private LoanApplication() { }

    public static LoanApplication Submit(Customer customer, decimal requestedAmount, DateTime nowUtc)
    {
        var loanApplication = new LoanApplication { Customer = customer, CustomerId = customer.Id };
        loanApplication.Resubmit(requestedAmount, nowUtc);
        return loanApplication;
    }

    /// <summary>The largest amount the <c>decimal(18,2)</c> column can hold.</summary>
    public const decimal MaxRequestedAmount = 9_999_999_999_999_999.99m;

    /// <returns>The error message, or <c>null</c> when the amount is valid.</returns>
    public static string? ValidateRequestedAmount(decimal? requestedAmount) => requestedAmount switch
    {
        null or <= 0 => "Requested amount must be greater than 0.",
        > MaxRequestedAmount => "Requested amount is too large.",
        { } amount when decimal.Round(amount, 2) != amount => "Use at most 2 decimal places.",
        _ => null,
    };

    public void Resubmit(decimal requestedAmount, DateTime nowUtc)
    {
        if (ValidateRequestedAmount(requestedAmount) is { } error)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedAmount), error);
        }

        RequestedAmount = requestedAmount;
        SubmittedAtUtc = nowUtc;
    }
}
