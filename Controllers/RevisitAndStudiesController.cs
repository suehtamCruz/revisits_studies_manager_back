using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using RevisitsStudiesManager.Back.DTOs;
using RevisitsStudiesManager.Back.Models;
using RevisitsStudiesManager.Back.Services;

namespace RevisitsStudiesManager.Back.Controllers;

/// <summary>
/// CRUD de revisitas e estudos. Requer autenticação via token JWT.
/// Todos os registros pertencem ao usuário autenticado — cada usuário
/// só vê e só pode alterar os próprios registros.
/// </summary>
[ApiController]
[Authorize]
[Route("api/revisitAndStudies")]
[Produces("application/json")]
public class RevisitAndStudiesController : ControllerBase
{
    private readonly IRevisitAndStudyService _service;

    public RevisitAndStudiesController(IRevisitAndStudyService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista as revisitas e estudos do usuário logado, com filtros opcionais.
    /// </summary>
    /// <param name="dayOfWeek">Filtra pelo dia da semana (0=Sunday .. 6=Saturday).</param>
    /// <param name="type">Filtra pelo tipo: Revisit ou Study.</param>
    /// <response code="200">Lista de registros do usuário logado.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RevisitAndStudy>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] DayOfWeek? dayOfWeek, [FromQuery] RevisitAndStudyType? type) =>
        Ok(await _service.GetAllAsync(GetUserId(), dayOfWeek, type));

    /// <summary>
    /// Busca um registro do usuário logado pelo id.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var item = await _service.GetByIdAsync(id, GetUserId());
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Cria uma nova revisita ou estudo vinculada ao usuário logado.
    /// </summary>
    /// <param name="request">Dados do registro.</param>
    /// <response code="201">Registro criado.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(RevisitAndStudyRequest request)
    {
        var item = await _service.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Atualiza parcialmente um registro do usuário logado.
    /// Envie apenas os campos que deseja alterar — os demais permanecem inalterados.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <param name="request">Campos a atualizar (todos opcionais).</param>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Nenhum campo enviado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(string id, RevisitAndStudyPatchRequest request)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var result = await _service.PatchAsync(id, request, GetUserId());
        return result.Status switch
        {
            PatchStatus.NotFound => NotFound(),
            PatchStatus.NothingToUpdate => BadRequest(new { message = "Envie ao menos um campo para atualizar." }),
            _ => Ok(result.Item)
        };
    }

    /// <summary>
    /// Remove um registro do usuário logado.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <response code="204">Registro removido.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        return await _service.DeleteAsync(id, GetUserId()) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Adiciona uma visita à lista de visitas do registro.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <param name="visit">Dados da visita.</param>
    /// <response code="200">Registro atualizado com a nova visita.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPost("{id}/visits")]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddVisit(string id, Visit visit)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var item = await _service.AddVisitAsync(id, visit, GetUserId());
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Lista as visitas de um registro do usuário logado, com paginação.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <param name="page">Número da página (default: 1).</param>
    /// <param name="take">Itens por página (default: 10, máx: 100).</param>
    /// <response code="200">Página de visitas com total, page e take.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id}/visits")]
    [ProducesResponseType(typeof(PagedResult<Visit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllVisits(string id, [FromQuery] int page = 1, [FromQuery] int take = 10)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var result = await _service.GetAllVisitsAsync(id, GetUserId(), page, take);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Altera o tipo do registro de Revisit para Study.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <response code="200">Registro atualizado com type = Study.</response>
    /// <response code="400">O registro já é Study.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPatch("{id}/type")]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeType(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var result = await _service.ChangeTypeToStudyAsync(id, GetUserId());
        return result.Status switch
        {
            ChangeTypeStatus.NotFound => NotFound(),
            ChangeTypeStatus.AlreadyStudy => BadRequest(new { message = "Somente possivel trocar de revisita para estudo" }),
            _ => Ok(result.Item)
        };
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub");
}
