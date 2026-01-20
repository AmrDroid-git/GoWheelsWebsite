using System.ComponentModel.DataAnnotations;

namespace GoWheels.ViewModels;

public class PostCreateViewModel
{
    [Required]
    [MaxLength(100)]
    public string Constructor { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    [Display(Name = "Model Name")]
    public string ModelName { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Release Date")]
    public DateOnly ReleaseDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Purchase Date")]
    public DateOnly PurchaseDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Kilometrage { get; set; }
    
    [Required]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    
    // public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    
    [Display(Name = "Upload Images")]
    public ICollection<IFormFile>? Images { get; set; } = new List<IFormFile>();
    
    [Required]
    public string OwnerId { get; set; } = string.Empty;

}