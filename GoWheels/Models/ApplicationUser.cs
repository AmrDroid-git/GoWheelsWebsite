using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // User's Average Rating (as a Seller) ---
        public float RateAverage { get; set; } = 0f;

        // --- Relationships ---
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
        public ICollection<RatingUser> ReceivedRatings { get; set; } = new List<RatingUser>();
    }
}