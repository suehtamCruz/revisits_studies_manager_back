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
    /// Lista revisitas e estudos, com filtros opcionais.
    /// </summary>
    /// <param name="dayOfWeek">Filtra pelo dia da semana (Sunday..Saturday).</param>
    /// <param name="type">Filtra pelo tipo: Revisit ou Study.</param>
    /// <response code="200">Lista de registros.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RevisitAndStudy>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] DayOfWeek? dayOfWeek, [FromQuery] RevisitAndStudyType? type)
    {
        var filter = Builders<RevisitAndStudy>.Filter.Empty;

        if (dayOfWeek.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.DayOfWeek, dayOfWeek.Value);

        if (type.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.Type, type.Value);

        return Ok(await _context.RevisitAndStudies.Find(filter).ToListAsync());
    }

    /// <summary>
    /// Busca um registro pelo id.
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
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Cria uma nova revisita ou estudo.
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
            Type = request.Type
        };

        await _context.RevisitAndStudies.InsertOneAsync(item);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Atualiza um registro existente.
    /// </summary>
    /// <param name="id">Id do registro (ObjectId do MongoDB).</param>
    /// <param name="request">Novos dados do registro.</param>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RevisitAndStudy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, RevisitAndStudyRequest request)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var item = new RevisitAndStudy
        {
            Id = id,
            Name = request.Name,
            Age = request.Age,
            DayOfWeek = request.DayOfWeek,
            Details = request.Details,
            Address = request.Address,
            Type = request.Type
        };

        var result = await _context.RevisitAndStudies.ReplaceOneAsync(r => r.Id == id, item);
        return result.MatchedCount == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Remove um registro.
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

        var result = await _context.RevisitAndStudies.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
