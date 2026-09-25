using System.Security.Claims;
using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Loans.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer(JwtOptions options, TimeProvider timeProvider) : IAccessTokenIssuer
{
    public string Issue(User user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Issuer,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            }),
            NotBefore = now,
            Expires = now.AddMinutes(options.LifetimeMinutes),
            SigningCredentials = new SigningCredentials(options.SigningKey, SecurityAlgorithms.HmacSha256),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
