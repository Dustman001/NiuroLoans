namespace Loans.Domain;

public sealed class BlacklistedSsn
{
    public int Id { get; private set; }
    public Ssn Ssn { get; private set; } = null!;
    public DateTime AddedAtUtc { get; private set; }

    private BlacklistedSsn() { }

    public BlacklistedSsn(Ssn ssn, DateTime addedAtUtc)
    {
        Ssn = ssn;
        AddedAtUtc = addedAtUtc;
    }
}
