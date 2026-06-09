namespace IBOPE.DoubleBlind.Domain.Entities;

public sealed class User
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string PasswordHash { get; init; }
    public bool IsActive { get; init; } = true;
}
