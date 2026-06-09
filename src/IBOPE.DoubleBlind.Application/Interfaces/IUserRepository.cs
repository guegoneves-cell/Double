using IBOPE.DoubleBlind.Domain.Entities;

namespace IBOPE.DoubleBlind.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
