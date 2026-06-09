using IBOPE.DoubleBlind.Application.DTOs;
using IBOPE.DoubleBlind.Application.Interfaces;

namespace IBOPE.DoubleBlind.Infrastructure.Services;

public sealed class AuthenticationService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResultDto> AuthenticateAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return LoginResultDto.Failure("Credenciais inválidas.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return LoginResultDto.Failure("Credenciais inválidas.");
        }

        return LoginResultDto.Success(user.Id, user.DisplayName);
    }
}
