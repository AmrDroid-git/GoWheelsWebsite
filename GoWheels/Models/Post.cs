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
    [Index(nameof(ReleaseDate))]
    public class Post
    {
        // --- General ---
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Created At")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }
        
        public PostStatus Status { get; set; } = PostStatus.Pending;
        public bool IsForRent { get; set; } = false;
        public float? RateAverage { get; set; }
        public int RatingsCount { get; set; } = 0;

        public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
        
        // --- Vehicle Details ---
        [Required]
        [MaxLength(100)]
        public string Constructor { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly ReleaseDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly PurchaseDate { get; set; }

        public int Kilometrage { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")] // (dinars,millimes)
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:N3} DT", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();

        // --- Relationships ---
        [Required]
        [MaxLength(128)]
        public string OwnerId { get; set; } = string.Empty;
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RatingPost> Ratings { get; set; } = new List<RatingPost>();
    }
}