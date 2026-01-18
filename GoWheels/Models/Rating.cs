using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    // Abstract Base Class
    public abstract class Rating
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Range(0, 5)]
        public float Value { get; set; } // 0, 0.5, ... 5

        // The person GIVING the rating
        [Required]
        [MaxLength(128)]
        public string OwnerId { get; set; } = null!;
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;
    }

    // Specific Rating for a User (Seller)
    public class RatingUser : Rating
    {
        // The person BEING rated
        [Required]
        [MaxLength(128)]
        public string RatedUserId { get; set; } = null!;
        [ForeignKey("RatedUserId")]
        public ApplicationUser RatedUser { get; set; } = null!;
    }

    // Specific Rating for a Post (Car)
    public class RatingPost : Rating
    {
        // The post BEING rated
        [Required]
        [MaxLength(128)]
        public string RatedPostId { get; set; } = null!;
        [ForeignKey("RatedPostId")]
        public Post RatedPost { get; set; } = null!;
    }
}