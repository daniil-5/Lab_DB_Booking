using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Other;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookingSystem.Infrastructure.Repositories;

public class MongoLogRepository : ILogRepository
{
    private readonly IMongoCollection<LogEntity> _logCollection;

    public MongoLogRepository(IConfiguration configuration, IOptions<MongoDbSettings> mongoDbSettings)
    {
        var settings = mongoDbSettings.Value;
        var connectionString = configuration.GetConnectionString("MongoDbConnection");
        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
        _logCollection = mongoDatabase.GetCollection<LogEntity>(settings.UserActionsCollectionName);
        
        var indexKeysDefinition = Builders<LogEntity>.IndexKeys.Ascending(x => x.Timestamp);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(settings.LogsTtlDays) };
        var indexModel = new CreateIndexModel<LogEntity>(indexKeysDefinition, indexOptions);
        _logCollection.Indexes.CreateOne(indexModel);
    }

    public async Task AddLogAsync(LogEntity log)
    {
        await _logCollection.InsertOneAsync(log);
    }

    public async Task<IEnumerable<LogEntity>> GetLogsAsync(DateTime? startDate, DateTime? endDate, int? userId, string? eventType)
    {
        var filterBuilder = Builders<LogEntity>.Filter;
        var filter = filterBuilder.Empty;

        if (startDate.HasValue)
        {
            filter &= filterBuilder.Gte(log => log.Timestamp, startDate.Value);
        }

        if (endDate.HasValue)
        {
            filter &= filterBuilder.Lte(log => log.Timestamp, endDate.Value);
        }

        if (userId.HasValue)
        {
            filter &= filterBuilder.Eq(log => log.UserId, userId.Value);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            filter &= filterBuilder.Eq(log => log.ActionType.ToString(), eventType);
        }

        return await _logCollection.Find(filter).ToListAsync();
    }
}
