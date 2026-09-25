using Loans.Application.LoanApplications;
using Loans.Application.LoanApplications.Rules;
using Loans.Domain;
using Loans.Tests.Fakes;

namespace Loans.Tests;

public sealed class LoanRuleEngineTests
{
    private readonly InMemoryDatabase _db = new();
    private readonly LoanRuleEngine _engine;

    public LoanRuleEngineTests()
    {
        _db.States.AddRange(TestData.States());
        _db.Blacklist.Add(new BlacklistedSsn(TestData.BlacklistedSsn, DateTime.UtcNow));
        _engine = new LoanRuleEngine([new StateAllowedRule(_db), new SsnNotBlacklistedRule(_db)]);
    }

    [Fact]
    public async Task Passes_when_state_is_allowed_and_ssn_is_not_blacklisted()
    {
        var denialReason = await Evaluate(TestData.CleanSsn, TestData.AllowedStateId);

        Assert.Null(denialReason);
    }

    [Fact]
    public async Task Denies_with_state_reason_when_state_is_not_allowed()
    {
        var denialReason = await Evaluate(TestData.CleanSsn, TestData.NotAllowedStateId);

        Assert.Equal(DenialReason.State, denialReason);
    }

    [Fact]
    public async Task Denies_with_state_reason_when_state_does_not_exist()
    {
        var denialReason = await Evaluate(TestData.CleanSsn, stateId: 999);

        Assert.Equal(DenialReason.State, denialReason);
    }

    [Fact]
    public async Task Denies_with_ssn_reason_when_ssn_is_blacklisted()
    {
        var denialReason = await Evaluate(TestData.BlacklistedSsn, TestData.AllowedStateId);

        Assert.Equal(DenialReason.Ssn, denialReason);
    }

    [Fact]
    public async Task Reports_the_first_failing_rule_when_several_fail()
    {
        var denialReason = await Evaluate(TestData.BlacklistedSsn, TestData.NotAllowedStateId);

        Assert.Equal(DenialReason.State, denialReason);
    }

    private Task<DenialReason?> Evaluate(Ssn ssn, int stateId) =>
        _engine.EvaluateAsync(new SubmitLoanApplication(ssn, TestData.Applicant(stateId), 1_000m), CancellationToken.None);
}
