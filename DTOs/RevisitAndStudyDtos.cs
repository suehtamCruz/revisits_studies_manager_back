using System.ComponentModel.DataAnnotations;
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

    /// <summary>Dia da semana (0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday).</summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Detalhes/observações (opcional).</summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>Endereço da pessoa.</summary>
    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Tipo: Revisit ou Study.</summary>
    public RevisitAndStudyType Type { get; set; }

    /// <summary>Data da última visita como string ISO 8601 (opcional). Ex: "2026-11-09" ou "2026-11-09T00:00:00.000Z".</summary>
    public DateTime? LastVisit { get; set; }
}

/// <summary>
/// Update parcial (PATCH): envie apenas os campos que deseja alterar.
/// Campos não enviados permanecem inalterados.
/// </summary>
public class RevisitAndStudyPatchRequest
{
    /// <summary>Nome da pessoa.</summary>
    [MaxLength(120)]
    public string? Name { get; set; }

    /// <summary>Idade da pessoa.</summary>
    [Range(0, 130)]
    public int? Age { get; set; }

    /// <summary>Dia da semana (0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday).</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>Detalhes/observações.</summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>Endereço da pessoa.</summary>
    [MaxLength(250)]
    public string? Address { get; set; }

    /// <summary>Tipo: Revisit ou Study.</summary>
    public RevisitAndStudyType? Type { get; set; }

    /// <summary>Data da última visita como string ISO 8601. Ex: "2026-11-09T00:00:00.000Z".</summary>
    public DateTime? LastVisit { get; set; }
}
