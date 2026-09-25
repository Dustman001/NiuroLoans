using Loans.Api.Contracts;
using Loans.Application.LoanApplications;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class LoanApplicationsController(LoanApplicationService loanApplicationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LoanApplicationResponse>> Submit(LoanApplicationRequest request, CancellationToken cancellationToken)
    {
        if (!request.TryCreateCommand(out var command, out var errors))
        {
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var decision = await loanApplicationService.SubmitAsync(command, cancellationToken);
        return new LoanApplicationResponse(decision.IsApproved, decision.DenialReason);
    }
}
