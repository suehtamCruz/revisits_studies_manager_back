using System.ComponentModel.DataAnnotations;

namespace RevisitsStudiesManager.Back.DTOs;

/// <summary>Dados para cadastro de um novo usuário.</summary>
public class RegisterRequest
{
    /// <summary>Nome completo.</summary>
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>E-mail válido e único.</summary>
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha (mínimo 6 caracteres). Será armazenada com hash BCrypt.</summary>
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Credenciais para autenticação.</summary>
public class LoginRequest
{
    /// <summary>E-mail cadastrado.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha cadastrada.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Resposta de autenticação bem-sucedida.</summary>
public class AuthResponse
{
    /// <summary>Token JWT. Use no header: Authorization: Bearer {token}</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Data/hora UTC de expiração do token.</summary>
    public DateTime ExpiresAt { get; set; }
}
