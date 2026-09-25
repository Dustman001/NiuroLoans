using Loans.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Loans.Api.Authentication;

public static class JwtAuthenticationExtensions
{
    /// <summary>Validates bearer tokens issued by <see cref="JwtAccessTokenIssuer"/>.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtOptions>((bearer, jwt) => bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Issuer,
                IssuerSigningKey = jwt.SigningKey,
            });
        services.AddAuthorization();
        return services;
    }
}
