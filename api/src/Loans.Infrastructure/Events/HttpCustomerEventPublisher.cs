using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Loans.Application.Abstractions;
using Loans.Application.LoanApplications;

namespace Loans.Infrastructure.Events;

/// <summary>Delivers customer events to the events service (API 2) over HTTP. Throws on any non-2xx.</summary>
public sealed class HttpCustomerEventPublisher(HttpClient httpClient) : ICustomerEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task PublishAsync(CustomerEvent customerEvent, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("events", customerEvent, SerializerOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
