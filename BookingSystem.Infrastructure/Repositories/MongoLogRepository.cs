using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Other;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

    public async Task<IEnumerable<Domain.DTOs.Reports.UserActivity>> GetUserActivityReportAsync(string period)
    {
        var groupBy = new BsonDocument();
        switch (period.ToLower())
        {
            case "day":
                groupBy = new BsonDocument
                {
                    { "year", new BsonDocument("$year", "$timestamp") },
                    { "month", new BsonDocument("$month", "$timestamp") },
                    { "day", new BsonDocument("$dayOfMonth", "$timestamp") }
                };
                break;
            case "week":
                groupBy = new BsonDocument
                {
                    { "year", new BsonDocument("$year", "$timestamp") },
                    { "week", new BsonDocument("$week", "$timestamp") }
                };
                break;
            case "month":
                groupBy = new BsonDocument
                {
                    { "year", new BsonDocument("$year", "$timestamp") },
                    { "month", new BsonDocument("$month", "$timestamp") }
                };
                break;
            default:
                throw new ArgumentException("Invalid period. Supported periods are 'day', 'week', 'month'.");
        }

        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", groupBy },
                { "TotalActions", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var result = await _logCollection.Aggregate<BookingSystem.Domain.DTOs.Reports.UserActivity>(pipeline).ToListAsync();
        return result;
    }

    public async Task<IEnumerable<BookingSystem.Domain.DTOs.Reports.TopUser>> GetTopUsersReportAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$user_id" },
                { "TotalActions", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "UserId", "$_id" },
                { "TotalActions", "$TotalActions" }
            }),
            new BsonDocument("$sort", new BsonDocument("TotalActions", -1)),
            new BsonDocument("$limit", 10)
        };

        var result = await _logCollection.Aggregate<BookingSystem.Domain.DTOs.Reports.TopUser>(pipeline).ToListAsync();
        return result;
    }

    public async Task<IEnumerable<BookingSystem.Domain.DTOs.Reports.OperationDistribution>> GetOperationDistributionReportAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$action_type" },
                { "Count", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("Count", -1))
        };

        var result = await _logCollection.Aggregate<BookingSystem.Domain.DTOs.Reports.OperationDistribution>(pipeline).ToListAsync();
        return result;
    }

    public async Task<IEnumerable<BookingSystem.Domain.DTOs.Reports.TimeSeries>> GetTimeSeriesReportAsync()
    {
        var groupBy = new BsonDocument
        {
            { "year", new BsonDocument("$year", "$timestamp") },
            { "month", new BsonDocument("$month", "$timestamp") },
            { "day", new BsonDocument("$dayOfMonth", "$timestamp") }
        };

        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", groupBy },
                { "TotalActions", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var result = await _logCollection.Aggregate<BookingSystem.Domain.DTOs.Reports.TimeSeries>(pipeline).ToListAsync();
        return result;
    }
}
