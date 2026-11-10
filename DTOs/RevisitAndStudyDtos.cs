using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RevisitsStudiesManager.Back.Models;

namespace RevisitsStudiesManager.Back.DTOs;

/// <summary>Dados para criar uma revisita ou estudo.</summary>
public class RevisitAndStudyRequest
{
    /// <summary>Nome da pessoa.</summary>
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Idade da pessoa.</summary>
    [Range(0, 130)]
    public int Age { get; set; }

    /// <summary>Dia da semana (Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Detalhes/observações (opcional).</summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>Endereço da pessoa.</summary>
    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Tipo: Revisit ou Study.</summary>
    public RevisitAndStudyType Type { get; set; }
}
