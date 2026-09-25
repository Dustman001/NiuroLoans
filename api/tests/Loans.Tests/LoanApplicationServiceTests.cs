using Loans.Application.LoanApplications;
using Loans.Application.LoanApplications.Rules;
using Loans.Domain;
using Loans.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Loans.Tests;

public sealed class LoanApplicationServiceTests
{
    private static readonly DateTime FirstSubmission = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryDatabase _db = new();
    private readonly FakeCustomerEventPublisher _publisher = new();
    private readonly LoanApplicationService _service;

    public LoanApplicationServiceTests()
    {
        _db.States.AddRange(TestData.States());
        _db.Blacklist.Add(new BlacklistedSsn(TestData.BlacklistedSsn, DateTime.UtcNow));
        var ruleEngine = new LoanRuleEngine([new StateAllowedRule(_db), new SsnNotBlacklistedRule(_db)]);
        _service = new LoanApplicationService(
            ruleEngine, _db, _db, _publisher, _db, TimeProvider.System, NullLogger<LoanApplicationService>.Instance);
    }

    [Fact]
    public async Task New_customer_is_saved_with_application_and_NewCustomer_event()
    {
        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(), 2_000m);

        Assert.True(decision.IsApproved);
        var customer = Assert.Single(_db.Customers);
        var loanApplication = Assert.Single(_db.LoanApplications);
        Assert.Equal(customer.Id, loanApplication.CustomerId);
        Assert.Equal(2_000m, loanApplication.RequestedAmount);
        var published = Assert.Single(_publisher.Published);
        Assert.Equal(CustomerEventType.NewCustomer, published.Type);
        Assert.Equal(TestData.CleanSsn.Value, published.Ssn);
    }

    [Fact]
    public async Task Returning_customer_updates_existing_rows_and_sends_ReturningCustomer_event()
    {
        var existing = _db.SeedCustomer(TestData.CleanSsn, TestData.Applicant(firstName: "Old"), 1_000m, FirstSubmission);

        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(firstName: "New"), 7_500m);

        Assert.True(decision.IsApproved);
        var customer = Assert.Single(_db.Customers);
        Assert.Same(existing, customer);
        Assert.Equal("New", customer.Details.FirstName);
        var loanApplication = Assert.Single(_db.LoanApplications);
        Assert.Equal(7_500m, loanApplication.RequestedAmount);
        Assert.True(loanApplication.SubmittedAtUtc > FirstSubmission);
        Assert.Equal(CustomerEventType.ReturningCustomer, Assert.Single(_publisher.Published).Type);
    }

    [Fact]
    public async Task Denied_application_saves_nothing_and_publishes_nothing()
    {
        var decision = await Submit(TestData.BlacklistedSsn, TestData.Applicant(), 2_000m);

        Assert.Equal(LoanDecision.Denial(DenialReason.Ssn), decision);
        Assert.Empty(_db.Customers);
        Assert.Equal(0, _publisher.Attempts);
    }

    [Fact]
    public async Task Publish_failure_is_retried_and_succeeds_within_three_attempts()
    {
        _publisher.FailuresBeforeSuccess = 2;

        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(), 2_000m);

        Assert.True(decision.IsApproved);
        Assert.Equal(3, _publisher.Attempts);
        Assert.Single(_db.Customers);
        Assert.Single(_db.LoanApplications);
    }

    [Fact]
    public async Task Publish_timeout_is_retried_like_any_other_failure()
    {
        _publisher.FailuresBeforeSuccess = 1;
        _publisher.Failure = () => new TaskCanceledException("The request timed out.");

        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(), 2_000m);

        Assert.True(decision.IsApproved);
        Assert.Equal(2, _publisher.Attempts);
    }

    [Fact]
    public async Task Database_outage_during_the_rules_is_retried_then_denied_as_unavailable()
    {
        _db.ReadFailure = new InvalidOperationException("Database unavailable.");

        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(), 2_000m);

        Assert.Equal(LoanDecision.Denial(DenialReason.Unavailable), decision);
        Assert.Equal(0, _publisher.Attempts);
    }

    [Fact]
    public async Task Publish_failing_three_times_rolls_back_and_denies_as_unavailable()
    {
        _publisher.FailuresBeforeSuccess = int.MaxValue;

        var decision = await Submit(TestData.CleanSsn, TestData.Applicant(), 2_000m);

        Assert.Equal(LoanDecision.Denial(DenialReason.Unavailable), decision);
        Assert.Equal(LoanApplicationService.MaxAttempts, _publisher.Attempts);
        Assert.Equal(LoanApplicationService.MaxAttempts, _db.Rollbacks);
        Assert.Equal(0, _db.Commits);
        Assert.Empty(_db.Customers);
        Assert.Empty(_db.LoanApplications);
    }

    private Task<LoanDecision> Submit(Ssn ssn, ApplicantDetails applicant, decimal amount) =>
        _service.SubmitAsync(new SubmitLoanApplication(ssn, applicant, amount), CancellationToken.None);
}
