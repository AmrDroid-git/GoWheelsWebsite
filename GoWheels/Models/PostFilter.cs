namespace GoWheels.Models
{
    public class PostFilter
    {
        // Transaction type: null=any, true=rent, false=sell
        public bool? IsForRent { get; set; }
        
        // Marque checklist
        public List<string>? Constructors { get; set; }
        
        // Modèle checklist
        public List<string>? Models { get; set; }
        
        // Price range
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        
        // Kilometrage range
        public int? KilometrageMin { get; set; }
        public int? KilometrageMax { get; set; }
        
        // Year range
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        
        // Rating: all, 4+, 5
        public string? RatingFilter { get; set; } = "all";
        
        // Status filter (all/active/pending/verified/refused/deleted)
        public string? StatusFilter { get; set; }
        
        // Pagination
        public int Page { get; set; } = 1;
        public const int PageSize = 20;
    }
}