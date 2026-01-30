using System.ComponentModel.DataAnnotations;
using GoWheels.Models;

namespace GoWheels.ViewModels;

public class PostEditViewModel
{
    // Required for form
    [Required]
    public string Id { get; set; } = string.Empty;
    
    // Basic info
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
    [Range(0.01, double.MaxValue)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    
    // Status - different logic for admin/user
    [Required]
    [Display(Name = "Status")]
    public PostStatus Status { get; set; }
    
    // IsForRent - only admin can change
    [Display(Name = "Transaction Type")]
    public bool IsForRent { get; set; }
    
    // Specifications
    public List<string> SpecificationKeys { get; set; } = new();
    public List<string> SpecificationValues { get; set; } = new();
    
    // Images
    public List<PostImageViewModel> ExistingImages { get; set; } = new();
    public ICollection<IFormFile>? NewImages { get; set; }
    
    // Read-only info for display
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public float? RateAverage { get; set; }
    public int RatingsCount { get; set; }
}

public class PostImageViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}