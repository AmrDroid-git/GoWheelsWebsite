using GoWheels.Models;
using MongoDB.Bson.Serialization.Attributes;

public class MongoPost
{
    [BsonId]
    public string Id { get; set; } = null!;

    public string Constructor { get; set; } = null!;
    public string ModelName { get; set; } = null!;

    public decimal Price { get; set; }
    public int Kilometrage { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsForRent { get; set; }
    public float? RateAverage { get; set; }
    public int RatingsCount { get; set; }

    public Dictionary<string, string> Specifications { get; set; } = new();

    public List<string> Images { get; set; } = new();

    public List<MongoCommentEmbedded> Comments { get; set; } = new();
}