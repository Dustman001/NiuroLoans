using Loans.Application.Abstractions;

namespace Loans.Application.Authentication;

public sealed class AuthService(IUserRepository users, IPasswordHasher passwordHasher, IAccessTokenIssuer tokenIssuer)
{
    /// <returns>An access token, or <c>null</c> when the credentials do not match a user.</returns>
    public async Task<string?> LogInAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim(), cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, password))
        {
            return null;
        }

        return tokenIssuer.Issue(user);
    }
}
