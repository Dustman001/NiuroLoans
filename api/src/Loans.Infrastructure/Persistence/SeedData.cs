using Loans.Domain;

namespace Loans.Infrastructure.Persistence;

public static class SeedData
{
    /// <summary>The 50 US states. New York starts as not allowed, as the spec requires.</summary>
    public static readonly State[] States =
    [
        new(1, "Alabama", "AL"),
        new(2, "Alaska", "AK"),
        new(3, "Arizona", "AZ"),
        new(4, "Arkansas", "AR"),
        new(5, "California", "CA"),
        new(6, "Colorado", "CO"),
        new(7, "Connecticut", "CT"),
        new(8, "Delaware", "DE"),
        new(9, "Florida", "FL"),
        new(10, "Georgia", "GA"),
        new(11, "Hawaii", "HI"),
        new(12, "Idaho", "ID"),
        new(13, "Illinois", "IL"),
        new(14, "Indiana", "IN"),
        new(15, "Iowa", "IA"),
        new(16, "Kansas", "KS"),
        new(17, "Kentucky", "KY"),
        new(18, "Louisiana", "LA"),
        new(19, "Maine", "ME"),
        new(20, "Maryland", "MD"),
        new(21, "Massachusetts", "MA"),
        new(22, "Michigan", "MI"),
        new(23, "Minnesota", "MN"),
        new(24, "Mississippi", "MS"),
        new(25, "Missouri", "MO"),
        new(26, "Montana", "MT"),
        new(27, "Nebraska", "NE"),
        new(28, "Nevada", "NV"),
        new(29, "New Hampshire", "NH"),
        new(30, "New Jersey", "NJ"),
        new(31, "New Mexico", "NM"),
        new(32, "New York", "NY", isNotAllowed: true),
        new(33, "North Carolina", "NC"),
        new(34, "North Dakota", "ND"),
        new(35, "Ohio", "OH"),
        new(36, "Oklahoma", "OK"),
        new(37, "Oregon", "OR"),
        new(38, "Pennsylvania", "PA"),
        new(39, "Rhode Island", "RI"),
        new(40, "South Carolina", "SC"),
        new(41, "South Dakota", "SD"),
        new(42, "Tennessee", "TN"),
        new(43, "Texas", "TX"),
        new(44, "Utah", "UT"),
        new(45, "Vermont", "VT"),
        new(46, "Virginia", "VA"),
        new(47, "Washington", "WA"),
        new(48, "West Virginia", "WV"),
        new(49, "Wisconsin", "WI"),
        new(50, "Wyoming", "WY"),
    ];

    public static int CaliforniaStateId => States.Single(s => s.Abbreviation == "CA").Id;

    public const string AdminEmail = "admin@niuro.test";
    public const string AdminPassword = "Admin123!";

    /// <summary>Blacklisted SSNs used by the README test scenarios.</summary>
    public static readonly string[] BlacklistedSsns = ["111-11-1111", "222-22-2222"];

    /// <summary>An existing customer, so the returning-customer path can be tried on first run.</summary>
    public const string ReturningCustomerSsn = "333-33-3333";
}
