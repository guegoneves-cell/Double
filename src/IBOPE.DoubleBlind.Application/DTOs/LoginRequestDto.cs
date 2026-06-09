using System.ComponentModel.DataAnnotations;

namespace IBOPE.DoubleBlind.Application.DTOs;

public sealed class LoginRequestDto
{
    [Required(ErrorMessage = "O login é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
