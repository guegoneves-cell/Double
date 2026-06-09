using IBOPE.DoubleBlind.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IBOPE.DoubleBlind.Infrastructure.Security;

public sealed class AspNetCorePasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password) =>
        _hasher.HashPassword(new object(), password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
