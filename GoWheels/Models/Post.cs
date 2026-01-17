using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoWheels.Models
{
    public enum PostStatus
    {
        Pending,
        Accepted,
        Rejected,
        Deleted
    }
    
    [Index(nameof(Constructor))]
    [Index(nameof(ModelName))]
    [Index(nameof(ReleaseYear))]
    public class Post
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public PostStatus Status { get; set; } = PostStatus.Pending;

        // --- Car Details ---
        [Required]
        public string Constructor { get; set; } = string.Empty;

        [Required]
        public string ModelName { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }
        public int BoughtYear { get; set; }
        public int Kilometrage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public float RateAverage { get; set; } = 0f;

        // --- Relationships ---
        [Required]
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RatingPost> Ratings { get; set; } = new List<RatingPost>();
    }
}