using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    public class AdminLog
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(128)]
        public string AdminId { get; set; } = null!;

        [ForeignKey(nameof(AdminId))]
        public ApplicationUser Admin { get; set; } = null!;
        
        [Required]
        [MaxLength(128)]
        public string UserId { get; set; } = null!;
        public ApplicationUser UserWhoDidTheAction { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = null!;

        [MaxLength(500)]
        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}