using Microsoft.AspNetCore.Identity;
using IPasswordHasher = Loans.Application.Abstractions.IPasswordHasher;

namespace Loans.Infrastructure.Security;

/// <summary>Salted PBKDF2 hashing from ASP.NET Core Identity, without the rest of Identity.</summary>
public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private static readonly object HashOwner = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(HashOwner, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(HashOwner, passwordHash, password) != PasswordVerificationResult.Failed;
}
