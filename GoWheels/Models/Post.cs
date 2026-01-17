using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Needed for Index attribute

namespace GoWheels.Models
{
    
    public enum PostStatus
    {
        Pending,
        Accepted,
        Rejected,
        Deleted
    }
    
    [Microsoft.EntityFrameworkCore.Index(nameof(Constructor))] // Explicitly specify the namespace
    [Microsoft.EntityFrameworkCore.Index(nameof(ModelName))]
    [Microsoft.EntityFrameworkCore.Index(nameof(ReleaseYear))]
    
    
    public class Post
    {
        [Key]
        public int Id { get; set; }
        
        // By default, a new post is always "Pending"
        public PostStatus Status { get; set; } = PostStatus.Pending;

        // --- Car Details ---
        [Required]
        public string Constructor { get; set; } = string.Empty; // Toyota

        [Required]
        public string ModelName { get; set; } = string.Empty; // Yaris

        public int ReleaseYear { get; set; } // 2005

        public int BoughtYear { get; set; } // 2006

        public int Kilometrage { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Better for currency than int
        public decimal Price { get; set; } // TND

        // --- JSONB Specification Map ---
        // This requires configuration in DbContext to work with Postgres
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Relationships ---

        // The Owner
        [Required]
        public string OwnerId { get; set; } // Identity uses String IDs
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;

        // Collections
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RatingPost> Ratings { get; set; } = new List<RatingPost>();
    }
}