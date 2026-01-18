using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GoWheels.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        // User's Average Rating (as a Seller) ---
        public float RateAverage { get; set; }

        // --- Relationships ---
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}