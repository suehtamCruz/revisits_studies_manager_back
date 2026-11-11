using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RevisitsStudiesManager.Back.Data;
using RevisitsStudiesManager.Back.DTOs;
using RevisitsStudiesManager.Back.Models;

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
    private readonly MongoDbContext _context;

    public RevisitAndStudiesController(MongoDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> GetAll([FromQuery] DayOfWeek? dayOfWeek, [FromQuery] RevisitAndStudyType? type)
    {
        var filter = Builders<RevisitAndStudy>.Filter.Eq(r => r.UserId, GetUserId());

        if (dayOfWeek.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.DayOfWeek, dayOfWeek.Value);

        if (type.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.Type, type.Value);

        return Ok(await _context.RevisitAndStudies.Find(filter).ToListAsync());
    }

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

        var item = await _context.RevisitAndStudies
            .Find(r => r.Id == id && r.UserId == GetUserId())
            .FirstOrDefaultAsync();

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
        var item = new RevisitAndStudy
        {
            Name = request.Name,
            Age = request.Age,
            DayOfWeek = request.DayOfWeek,
            Details = request.Details,
            Address = request.Address,
            Type = request.Type,
            LastVisit = request.LastVisit,
            UserId = GetUserId()
        };

        await _context.RevisitAndStudies.InsertOneAsync(item);

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

        var update = Builders<RevisitAndStudy>.Update;
        var updates = new List<UpdateDefinition<RevisitAndStudy>>();

        if (request.Name is not null) updates.Add(update.Set(r => r.Name, request.Name));
        if (request.Age.HasValue) updates.Add(update.Set(r => r.Age, request.Age.Value));
        if (request.DayOfWeek.HasValue) updates.Add(update.Set(r => r.DayOfWeek, request.DayOfWeek.Value));
        if (request.Details is not null) updates.Add(update.Set(r => r.Details, request.Details));
        if (request.Address is not null) updates.Add(update.Set(r => r.Address, request.Address));
        if (request.Type.HasValue) updates.Add(update.Set(r => r.Type, request.Type.Value));
        if (request.LastVisit.HasValue) updates.Add(update.Set(r => r.LastVisit, request.LastVisit.Value));

        if (updates.Count == 0)
            return BadRequest(new { message = "Envie ao menos um campo para atualizar." });

        var filter = Builders<RevisitAndStudy>.Filter.Where(
            r => r.Id == id && r.UserId == GetUserId());

        var result = await _context.RevisitAndStudies
            .UpdateOneAsync(filter, update.Combine(updates));

        if (result.MatchedCount == 0) return NotFound();

        var item = await _context.RevisitAndStudies
            .Find(filter)
            .FirstOrDefaultAsync();

        return Ok(item);
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

        var result = await _context.RevisitAndStudies
            .DeleteOneAsync(r => r.Id == id && r.UserId == GetUserId());

        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub");
}
