using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RevisitsStudiesManager.Back.Models;

/// <summary>
/// Tipo do registro: revisit (revisita) ou study (estudo).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevisitAndStudyType
{
    Revisit,
    Study
}

/// <summary>
/// Registro de revisita ou estudo dirigido a uma pessoa.
/// </summary>
public class RevisitAndStudy
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>Nome da pessoa.</summary>
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Idade da pessoa.</summary>
    [Range(0, 130)]
    public int Age { get; set; }

    /// <summary>Dia da semana da visita/estudo (0=Sunday .. 6=Saturday).</summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Detalhes/observações sobre o registro.</summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>Endereço da pessoa.</summary>
    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Tipo do registro: Revisit ou Study.</summary>
    [BsonRepresentation(BsonType.String)]
    public RevisitAndStudyType Type { get; set; }

    /// <summary>Id do usuário que criou o registro.</summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }
}
