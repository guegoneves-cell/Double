using IBOPE.DoubleBlind.Application.Interfaces;
using IBOPE.DoubleBlind.Domain.Entities;

namespace IBOPE.DoubleBlind.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly IReadOnlyList<User> _users;

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        _users =
        [
            new User
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Email = "admin",
                DisplayName = "Administrador",
                PasswordHash = passwordHasher.HashPassword("admin")
            }
        ];
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = _users.FirstOrDefault(u =>
            u.IsActive && string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }
}
