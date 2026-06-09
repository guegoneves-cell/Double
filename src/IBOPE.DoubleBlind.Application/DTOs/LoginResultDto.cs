namespace IBOPE.DoubleBlind.Application.DTOs;

public sealed class LoginResultDto
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? UserId { get; init; }
    public string? DisplayName { get; init; }

    public static LoginResultDto Success(Guid userId, string displayName) =>
        new() { Succeeded = true, UserId = userId, DisplayName = displayName };

    public static LoginResultDto Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
