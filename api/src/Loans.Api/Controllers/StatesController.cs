using Loans.Api.Contracts;
using Loans.Application.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Api.Controllers;

[ApiController]
[Route("api/states")]
public sealed class StatesController(StateService stateService) : ControllerBase
{
    /// <summary>Public: the application form needs the list of states.</summary>
    [HttpGet]
    public async Task<IEnumerable<StateResponse>> List(CancellationToken cancellationToken) =>
        (await stateService.ListAsync(cancellationToken))
            .Select(s => new StateResponse(s.Id, s.Name, s.Abbreviation, s.IsNotAllowed));

    [Authorize]
    [HttpPut("{stateId:int}")]
    public async Task<IActionResult> Update(int stateId, UpdateStateRequest request, CancellationToken cancellationToken)
    {
        if (request.IsNotAllowed is not { } isNotAllowed)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["isNotAllowed"] = ["This field is required."],
            }));
        }

        return await stateService.SetNotAllowedAsync(stateId, isNotAllowed, cancellationToken) ? NoContent() : NotFound();
    }
}
