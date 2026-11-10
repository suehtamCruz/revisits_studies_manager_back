using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RevisitsStudiesManager.Back.Models;

/// <summary>
/// Usuário do sistema, utilizado para autenticação via JWT.
/// </summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>Nome completo do usuário.</summary>
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>E-mail único usado no login.</summary>
    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt da senha (nunca armazenamos a senha em texto puro).</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
