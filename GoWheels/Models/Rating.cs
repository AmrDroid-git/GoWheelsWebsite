using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    // Abstract Base Class
    public abstract class Rating
    {
        [Key]
        public int Id { get; set; }

        [Range(0, 5)]
        public float Value { get; set; } // 0, 0.5, ... 5

        // The person GIVING the rating
        [Required]
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;
    }

    // Specific Rating for a User (Seller)
    public class RatingUser : Rating
    {
        // The person BEING rated
        public string RatedUserId { get; set; }
        [ForeignKey("RatedUserId")]
        public ApplicationUser RatedUser { get; set; } = null!;
    }

    // Specific Rating for a Post (Car)
    public class RatingPost : Rating
    {
        // The post BEING rated
        public int RatedPostId { get; set; }
        [ForeignKey("RatedPostId")]
        public Post RatedPost { get; set; } = null!;
    }
}