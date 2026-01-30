using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GoWheels.ViewModels;

public class PostCreateViewModel
{
    [Required(ErrorMessage = "Constructor is required")]
    [MaxLength(100)]
    public string Constructor { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Model name is required")]
    [MaxLength(100)]
    [Display(Name = "Model Name")]
    public string ModelName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Release date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Release Date")]
    public DateOnly ReleaseDate { get; set; }

    [Required(ErrorMessage = "Purchase date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Purchase Date")]
    public DateOnly PurchaseDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Kilometrage must be positive")]
    public int Kilometrage { get; set; }
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Please select transaction type")]
    [Display(Name = "Transaction Type")]
    public bool IsForRent { get; set; } = false;
    
    // Make specifications optional (just suggested)
    public List<string>? SpecificationKeys { get; set; }
    public List<string>? SpecificationValues { get; set; }
    
    [Display(Name = "Upload Images")]
    public ICollection<IFormFile>? Images { get; set; }
}