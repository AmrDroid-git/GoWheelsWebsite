using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    public class PostImage
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ImageUrl { get; set; } = null!;

        // This links the image to the specific Post
        [Required]
        [MaxLength(128)]
        public string PostId { get; set; } = null!;
        
        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;
    }
}