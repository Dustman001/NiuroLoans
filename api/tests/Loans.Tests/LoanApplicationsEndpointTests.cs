using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Loans.Application.LoanApplications;

namespace Loans.Tests;

public sealed class LoanApplicationsEndpointTests(LoansApiFactory factory) : IClassFixture<LoansApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Approves_a_valid_application_and_publishes_a_NewCustomer_event()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", ValidRequest("555-55-5555"));

        var body = await ReadBody(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("approved").GetBoolean());
        Assert.Contains(factory.Database.Customers, c => c.Ssn.Value == "555555555");
        var published = factory.Publisher.Published.Single(e => e.Ssn == "555555555");
        Assert.Equal(CustomerEventType.NewCustomer, published.Type);
    }

    [Fact]
    public async Task Denies_with_State_reason_for_a_not_allowed_state()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", ValidRequest("666-66-6666", TestData.NotAllowedStateId));

        var body = await ReadBody(response);
        Assert.False(body.GetProperty("approved").GetBoolean());
        Assert.Equal("State", body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Denies_with_Ssn_reason_for_a_blacklisted_ssn()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", ValidRequest(TestData.BlacklistedSsn.ToString()));

        var body = await ReadBody(response);
        Assert.False(body.GetProperty("approved").GetBoolean());
        Assert.Equal("Ssn", body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Rejects_invalid_input_with_400()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", ValidRequest("12-34") with { Email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadBody(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("ssn", out _));
        Assert.True(errors.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Rejects_a_name_longer_than_the_database_column_with_400()
    {
        var request = ValidRequest("777-77-7777") with { FirstName = new string('a', 101) };

        var response = await _client.PostAsJsonAsync("/api/applications", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await ReadBody(response)).GetProperty("errors").TryGetProperty("firstName", out _));
    }

    [Fact]
    public async Task Configuration_endpoints_require_a_token()
    {
        var response = await _client.GetAsync("/api/blacklist");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static ApplicationPayload ValidRequest(string ssn, int stateId = TestData.AllowedStateId) =>
        new("Ana", "Lopez", "ana@example.com", "1 Market St", null, stateId, "94105", "Acme", 2_500m, ssn);

    private static async Task<JsonElement> ReadBody(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private sealed record ApplicationPayload(
        string FirstName, string LastName, string Email, string AddressLine1, string? AddressLine2,
        int StateId, string ZipCode, string CompanyName, decimal RequestedAmount, string Ssn);
}
