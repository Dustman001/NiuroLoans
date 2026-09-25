using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Loans.Infrastructure.Security;

/// <summary>JWT settings from the "Jwt" configuration section.</summary>
public sealed record JwtOptions(string Issuer, string Key, int LifetimeMinutes)
{
    public const string SectionName = "Jwt";

    public SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(Key));
}
