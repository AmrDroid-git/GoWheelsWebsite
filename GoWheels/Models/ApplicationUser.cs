using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    // Inherits from IdentityUser to get standard login features
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Note: IdentityUser already has a "PhoneNumber" property (string).
        // We will use the built-in one.

        // --- Relationships ---
        
        // A user can create many posts
        public ICollection<Post> Posts { get; set; } = new List<Post>();

        // A user can write many comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Ratings GIVEN by this user
        public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();

        // Ratings RECEIVED by this user (as a seller)
        public ICollection<RatingUser> ReceivedRatings { get; set; } = new List<RatingUser>();
    }
}