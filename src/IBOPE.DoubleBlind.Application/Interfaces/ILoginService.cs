using IBOPE.DoubleBlind.Application.DTOs;

namespace IBOPE.DoubleBlind.Application.Interfaces;

public interface ILoginService
{
    Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
