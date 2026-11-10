using MongoDB.Driver;
using RevisitsStudiesManager.Back.Models;

namespace RevisitsStudiesManager.Back.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<RevisitAndStudy> RevisitAndStudies =>
        _database.GetCollection<RevisitAndStudy>("revisitAndStudies");
}
