using Loans.Api.Contracts;
using Loans.Application.Configuration;
using Loans.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/blacklist")]
public sealed class BlacklistController(BlacklistService blacklistService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<BlacklistEntryResponse>> List(CancellationToken cancellationToken) =>
        (await blacklistService.ListAsync(cancellationToken)).Select(ssn => new BlacklistEntryResponse(ssn.ToString()));

    [HttpPost]
    public async Task<IActionResult> Add(BlacklistEntryRequest request, CancellationToken cancellationToken)
    {
        if (!Ssn.TryParse(request.Ssn, out var ssn))
        {
            return InvalidSsn();
        }

        return await blacklistService.AddAsync(ssn, cancellationToken)
            ? Created($"/api/blacklist/{ssn}", new BlacklistEntryResponse(ssn.ToString()))
            : Conflict();
    }

    [HttpDelete("{ssn}")]
    public async Task<IActionResult> Remove(string ssn, CancellationToken cancellationToken)
    {
        if (!Ssn.TryParse(ssn, out var parsed))
        {
            return InvalidSsn();
        }

        return await blacklistService.RemoveAsync(parsed, cancellationToken) ? NoContent() : NotFound();
    }

    private BadRequestObjectResult InvalidSsn() =>
        BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["ssn"] = [Ssn.FormatErrorMessage],
        }));
}
