using GoWheels.Data;
using GoWheels.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GoWheels.Services;

public class MongoPostsReadService
{
    private readonly IMongoCollection<MongoPost> _collection;

    public MongoPostsReadService(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<MongoPost>("posts");
    }

    // Trending posts
    public async Task<List<MongoPost>> GetTrendingPostsAsync(int limit = 20)
    {
        return await _collection
            .Find(p => p.RateAverage != null)
            .SortByDescending(p => p.RateAverage)
            .ThenByDescending(p => p.RatingsCount)
            .Limit(limit)
            .ToListAsync();
    }

    //  Search by constructor
    public async Task<List<MongoPost>> GetByConstructorAsync(string constructor)
    {
        return await _collection
            .Find(p => p.Constructor == constructor)
            .ToListAsync();
    }

    // Search by price range
    public async Task<List<MongoPost>> GetByPriceRangeAsync(decimal min, decimal max)
    {
        return await _collection
            .Find(p => p.Price >= min && p.Price <= max)
            .ToListAsync();
    }

    //  Full text search simple
    public async Task<List<MongoPost>> SearchAsync(string keyword)
    {
        var filter = Builders<MongoPost>.Filter.Or(
            Builders<MongoPost>.Filter.Regex(p => p.ModelName, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
            Builders<MongoPost>.Filter.Regex(p => p.Constructor, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
        );

        return await _collection.Find(filter).ToListAsync();
    }

    //  Get one post with embedded comments
    public async Task<MongoPost?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync();
    }
}