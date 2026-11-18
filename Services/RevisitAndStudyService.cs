using MongoDB.Driver;
using RevisitsStudiesManager.Back.Data;
using RevisitsStudiesManager.Back.DTOs;
using RevisitsStudiesManager.Back.Models;

namespace RevisitsStudiesManager.Back.Services;

public enum PatchStatus { Ok, NotFound, NothingToUpdate }

public record PatchResult(PatchStatus Status, RevisitAndStudy? Item);

public enum ChangeTypeStatus { Ok, NotFound, AlreadyStudy }

public record ChangeTypeResult(ChangeTypeStatus Status, RevisitAndStudy? Item);

public record PagedResult<T>(int Page, int Take, int Total, int TotalPages, List<T> Items);

public interface IRevisitAndStudyService
{
    Task<List<RevisitAndStudy>> GetAllAsync(string? userId, DayOfWeek? dayOfWeek, RevisitAndStudyType? type);
    Task<RevisitAndStudy?> GetByIdAsync(string id, string? userId);
    Task<RevisitAndStudy> CreateAsync(RevisitAndStudyRequest request, string? userId);
    Task<PatchResult> PatchAsync(string id, RevisitAndStudyPatchRequest request, string? userId);
    Task<bool> DeleteAsync(string id, string? userId);
    Task<RevisitAndStudy?> AddVisitAsync(string id, Visit visit, string? userId);
    Task<PagedResult<Visit>?> GetAllVisitsAsync(string id, string? userId, int page, int take);
    Task<ChangeTypeResult> ChangeTypeToStudyAsync(string id, string? userId);
}

public class RevisitAndStudyService : IRevisitAndStudyService
{
    private readonly MongoDbContext _context;

    public RevisitAndStudyService(MongoDbContext context)
    {
        _context = context;
    }

    private static FilterDefinition<RevisitAndStudy> OwnedFilter(string id, string? userId) =>
        Builders<RevisitAndStudy>.Filter.Where(r => r.Id == id && r.UserId == userId);

    public async Task<List<RevisitAndStudy>> GetAllAsync(string? userId, DayOfWeek? dayOfWeek, RevisitAndStudyType? type)
    {
        var filter = Builders<RevisitAndStudy>.Filter.Eq(r => r.UserId, userId);

        if (dayOfWeek.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.DayOfWeek, dayOfWeek.Value);

        if (type.HasValue)
            filter &= Builders<RevisitAndStudy>.Filter.Eq(r => r.Type, type.Value);

        return await _context.RevisitAndStudies.Find(filter).ToListAsync();
    }

    public async Task<RevisitAndStudy?> GetByIdAsync(string id, string? userId) =>
        await _context.RevisitAndStudies
            .Find(OwnedFilter(id, userId))
            .FirstOrDefaultAsync();

    public async Task<RevisitAndStudy> CreateAsync(RevisitAndStudyRequest request, string? userId)
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
            UserId = userId
        };

        if (request.LastVisit.HasValue)
        {
            item.Visits.Add(new Visit
            {
                Date = request.LastVisit.Value.ToString("o"),
                Details = request.Details
            });
        } else 
        {
            item.Visits.Add(new Visit
            {
                Date = DateTime.UtcNow.ToString("o"),
                Details = request.Details
            });
        }

        await _context.RevisitAndStudies.InsertOneAsync(item);
        return item;
    }

    public async Task<PatchResult> PatchAsync(string id, RevisitAndStudyPatchRequest request, string? userId)
    {
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
            return new PatchResult(PatchStatus.NothingToUpdate, null);

        var item = await _context.RevisitAndStudies.FindOneAndUpdateAsync(
            OwnedFilter(id, userId),
            update.Combine(updates),
            new FindOneAndUpdateOptions<RevisitAndStudy> { ReturnDocument = ReturnDocument.After });

        return item is null
            ? new PatchResult(PatchStatus.NotFound, null)
            : new PatchResult(PatchStatus.Ok, item);
    }

    public async Task<bool> DeleteAsync(string id, string? userId)
    {
        var result = await _context.RevisitAndStudies.DeleteOneAsync(OwnedFilter(id, userId));
        return result.DeletedCount > 0;
    }

    public async Task<RevisitAndStudy?> AddVisitAsync(string id, Visit visit, string? userId) =>
        await _context.RevisitAndStudies.FindOneAndUpdateAsync(
            OwnedFilter(id, userId),
            Builders<RevisitAndStudy>.Update.Push(r => r.Visits, visit),
            new FindOneAndUpdateOptions<RevisitAndStudy> { ReturnDocument = ReturnDocument.After });

    public async Task<PagedResult<Visit>?> GetAllVisitsAsync(string id, string? userId, int page, int take)
    {
        var item = await _context.RevisitAndStudies
            .Find(OwnedFilter(id, userId))
            .FirstOrDefaultAsync();

        if (item is null) return null;

        page = Math.Max(page, 1);
        take = Math.Clamp(take, 1, 100);

        var visits = item.Visits
            .Skip((page - 1) * take)
            .Take(take)
            .ToList();

        var totalPages = (int)Math.Ceiling(item.Visits.Count / (double)take);
        return new PagedResult<Visit>(page, take, item.Visits.Count, totalPages, visits);
    }

    public async Task<ChangeTypeResult> ChangeTypeToStudyAsync(string id, string? userId)
    {
        var item = await _context.RevisitAndStudies
            .Find(OwnedFilter(id, userId))
            .FirstOrDefaultAsync();

        if (item is null) return new ChangeTypeResult(ChangeTypeStatus.NotFound, null);

        if (item.Type == RevisitAndStudyType.Study)
            return new ChangeTypeResult(ChangeTypeStatus.AlreadyStudy, item);

        var updated = await _context.RevisitAndStudies.FindOneAndUpdateAsync(
            OwnedFilter(id, userId),
            Builders<RevisitAndStudy>.Update.Set(r => r.Type, RevisitAndStudyType.Study),
            new FindOneAndUpdateOptions<RevisitAndStudy> { ReturnDocument = ReturnDocument.After });

        return new ChangeTypeResult(ChangeTypeStatus.Ok, updated);
    }
}
