using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoWheels.Models
{
    public class AdminLog
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        
      
        [MaxLength(128)]
        public string ActorId { get; set; } = null!;
        [ForeignKey(nameof(ActorId))]
        public ApplicationUser Actor { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = null!;

        [MaxLength(500)]
        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}