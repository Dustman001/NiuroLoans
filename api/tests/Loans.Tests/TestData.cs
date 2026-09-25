using Loans.Domain;

namespace Loans.Tests;

public static class TestData
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Test-Password-1";

    public const int AllowedStateId = 1;
    public const int NotAllowedStateId = 2;

    public static readonly Ssn CleanSsn = Ssn.Parse("123-45-6789");
    public static readonly Ssn BlacklistedSsn = Ssn.Parse("111-11-1111");

    public static State[] States() =>
    [
        new(AllowedStateId, "California", "CA"),
        new(NotAllowedStateId, "New York", "NY", isNotAllowed: true),
    ];

    public static ApplicantDetails Applicant(int stateId = AllowedStateId, string firstName = "Ana") =>
        new(firstName, "Lopez", "ana@example.com", "1 Market St", null, stateId, "94105", "Acme");
}
