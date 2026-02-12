namespace GoWheels.Models;

public class MongoCommentEmbedded
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
