using GoWheels.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GoWheels.Data
{
    public class MongoMirrorService
    {
        private readonly IMongoDatabase _database;

        public MongoMirrorService(IOptions<MongoSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        // -------------------------------------------------------
        // Mirror POST (insert or update full document)
        // -------------------------------------------------------
        public async Task MirrorPostAsync(Post post)
        {
            var collection = _database.GetCollection<MongoPost>("posts");

            var mongoPost = new MongoPost
            {
                Id = post.Id,
                Constructor = post.Constructor,
                ModelName = post.ModelName,
                Price = post.Price,
                Kilometrage = post.Kilometrage,
                CreatedAt = post.CreatedAt,
                IsForRent = post.IsForRent,
                RateAverage = post.RateAverage,
                RatingsCount = post.RatingsCount,
                Specifications = post.Specifications ?? new(),
                Images = post.PostImages?.Select(i => i.ImageUrl).ToList() ?? new(),
                Comments = post.Comments?.Select(c => new MongoCommentEmbedded
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new()
            };

            await collection.ReplaceOneAsync(
                p => p.Id == mongoPost.Id,
                mongoPost,
                new ReplaceOptions { IsUpsert = true }
            );
        }

        // -------------------------------------------------------
        // Mirror COMMENT (push embedded comment)
        // -------------------------------------------------------
        public async Task MirrorCommentAsync(Comment comment)
        {
            var collection = _database.GetCollection<MongoPost>("posts");

            var embeddedComment = new MongoCommentEmbedded
            {
                Id = comment.Id,
                UserId = comment.UserId,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt
            };

            var update = Builders<MongoPost>.Update.Push(p => p.Comments, embeddedComment);

            await collection.UpdateOneAsync(
                p => p.Id == comment.PostId,
                update
            );
        }

        // -------------------------------------------------------
        // Mirror Rating (update summary only)
        // -------------------------------------------------------
        public async Task MirrorRatingAsync(Post post)
        {
            var collection = _database.GetCollection<MongoPost>("posts");

            var update = Builders<MongoPost>.Update
                .Set(p => p.RateAverage, post.RateAverage)
                .Set(p => p.RatingsCount, post.RatingsCount);

            await collection.UpdateOneAsync(
                p => p.Id == post.Id,
                update
            );
        }
    }
}
