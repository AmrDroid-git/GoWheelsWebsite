using System.ComponentModel.DataAnnotations;

namespace GoWheels.ViewModels
{
    public class UserEditViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [Length(8, 15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;
    }
}