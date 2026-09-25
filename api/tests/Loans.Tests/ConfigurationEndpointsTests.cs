using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Loans.Tests;

public sealed class ConfigurationEndpointsTests(LoansApiFactory factory) : IClassFixture<LoansApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LogIn_with_valid_credentials_returns_a_token()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = TestData.AdminEmail, password = TestData.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task LogIn_with_a_wrong_password_returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = TestData.AdminEmail, password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Blacklist_adds_rejects_duplicates_and_removes()
    {
        await AuthorizeAsync();
        const string ssn = "888-77-6666";

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/blacklist", new { ssn })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/blacklist", new { ssn })).StatusCode);
        Assert.Contains(await ListBlacklistAsync(), entry => entry == ssn);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/blacklist/{ssn}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/blacklist/{ssn}")).StatusCode);
        Assert.DoesNotContain(await ListBlacklistAsync(), entry => entry == ssn);
    }

    [Fact]
    public async Task Blacklist_rejects_an_invalid_ssn_with_400()
    {
        await AuthorizeAsync();

        var response = await _client.PostAsJsonAsync("/api/blacklist", new { ssn = "12-34" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task State_update_saves_the_flag_and_validates_the_request()
    {
        await AuthorizeAsync();
        var url = $"/api/states/{TestData.AllowedStateId}";

        Assert.Equal(HttpStatusCode.NoContent, (await _client.PutAsJsonAsync(url, new { isNotAllowed = true })).StatusCode);
        Assert.True(factory.Database.States.Single(s => s.Id == TestData.AllowedStateId).IsNotAllowed);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.PutAsJsonAsync(url, new { isNotAllowed = false })).StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PutAsJsonAsync(url, new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/states/999", new { isNotAllowed = true })).StatusCode);
    }

    private async Task AuthorizeAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = TestData.AdminEmail, password = TestData.AdminPassword });
        var token = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string[]> ListBlacklistAsync() =>
        (await _client.GetFromJsonAsync<JsonElement[]>("/api/blacklist"))!
            .Select(entry => entry.GetProperty("ssn").GetString()!)
            .ToArray();
}
