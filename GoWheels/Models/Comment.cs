using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    public class Comment
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(600)]
        public string Body { get; set; } = string.Empty;
        
        [Display(Name = "Created At")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        // --- Relationships ---

        // Link to Post
        [MaxLength(128)]
        public string PostId { get; set; } = null!;
        [ForeignKey("PostId")]
        public Post Post { get; set; } = null!;

        // Link to User (Author)
        [MaxLength(128)]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}