using Loans.Api.Contracts;
using Loans.Application.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LogInResponse>> LogIn(LogInRequest request, CancellationToken cancellationToken)
    {
        var token = await authService.LogInAsync(request.Email, request.Password, cancellationToken);
        if (token is null)
        {
            return Unauthorized();
        }

        return new LogInResponse(token);
    }
}
