using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services
{
    public class PostsService : IPostsService
    {
        private readonly GoWheelsDbContext _context;

        public PostsService(GoWheelsDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. GET POST BY ID
        // ==========================================================
        public async Task<Post?> GetPostByIdAsync(string id)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.PostImages)
                .Include(p => p.Ratings)
                    .ThenInclude(r => r.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ==========================================================
        // 2. ADD POST
        // ==========================================================
        public async Task<bool> AddPostAsync(Post post)
        {
            try
            {
                if (post.Status == 0) post.Status = PostStatus.Pending;
                post.CreatedAt = DateTime.UtcNow;

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post: {ex.Message}");
                return false;
            }
        }

        // ==========================================================
        // 3. UPDATE POST
        // ==========================================================
        public async Task<bool> UpdatePostAsync(Post post)
        {
            try
            {
                if (!_context.Posts.Any(p => p.Id == post.Id))
                {
                    return false;
                }

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                return false;
            }
        }

        // ==========================================================
        // 4. DELETE POST
        // ==========================================================
        public async Task<bool> DeletePostAsync(string id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return false;
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting post: {ex.Message}");
                return false;
            }
        }
        
        // ==========================================================
        // 5. GET COMMENTS FOR A POST
        // ==========================================================
        public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // ==========================================================
        // 6. GET RATINGS FOR A POST
        // ==========================================================
        public async Task<List<RatingPost>> GetRatingsByPostIdAsync(string postId)
        {
            return await _context.PostsRatings 
                .Where(r => r.RatedPostId == postId)
                .Include(r => r.Owner)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }
        
        // ==========================================================
        // 7. SEARCH & TEXT FILTERS
        // ==========================================================
        
        public async Task<List<Post>> SearchPostsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Post>();

            keyword = keyword.ToLower();

            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Constructor.ToLower().Contains(keyword) || 
                            p.ModelName.ToLower().Contains(keyword))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByConstructorAsync(string constructor)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Constructor.ToLower().Contains(constructor.ToLower()))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByModelAsync(string model)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.ModelName.ToLower().Contains(model.ToLower()))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ==========================================================
        // 8. RANGE FILTERS (Price, Year, Km, Date)
        // ==========================================================
        
        public async Task<List<Post>> GetPostsByOwnerAsync(string userId)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.OwnerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.OrderBy(p => p.Price).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByKilometrageRangeAsync(int? minKm, int? maxKm)
        {
            var query = _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .AsQueryable();

            if (minKm.HasValue)
                query = query.Where(p => p.Kilometrage >= minKm.Value);

            if (maxKm.HasValue)
                query = query.Where(p => p.Kilometrage <= maxKm.Value);

            return await query.OrderBy(p => p.Kilometrage).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByYearRangeAsync(int? minYear, int? maxYear)
        {
            var query = _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .AsQueryable();

            if (minYear.HasValue)
                query = query.Where(p => p.ReleaseDate >= new DateOnly(minYear.Value, 1, 1));

            if (maxYear.HasValue)
                query = query.Where(p => p.ReleaseDate <= new DateOnly(maxYear.Value, 12, 31));

            return await query.OrderByDescending(p => p.ReleaseDate).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ==========================================================
        // 9. SPECIAL FILTERS (JSON & State)
        // ==========================================================

        public async Task<List<Post>> GetPostsBySpecificationAsync(string specKey, string specValue)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Specifications[specKey] == specValue)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByStateAsync(PostStatus status)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ==========================================================
        // 10. SORTING (Recent & Top Rated)
        // ==========================================================

        public async Task<List<Post>> GetRecentPostsAsync(int count)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Status == PostStatus.Accepted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Post>> GetTopRatedPostsAsync(int count)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Where(p => p.Status == PostStatus.Accepted)
                .OrderByDescending(p => p.RateAverage)
                .Take(count)
                .ToListAsync();
        }
        
        // ==========================================================
        // 11. ROLE-BASED LOGIC (Validation & Owner)
        // ==========================================================

        public async Task<bool> ValidatePostAsync(string postId, PostStatus status, string expertId)
        {
            try
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post == null) return false;

                post.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating post: {ex.Message}");
                return false;
            }
        }
        
        // ==========================================================
        // 12. UTILITIES (Intersection)
        // ==========================================================

        public List<Post> IntersectPosts(List<List<Post>> listsOfPosts)
        {
            if (listsOfPosts == null || !listsOfPosts.Any())
            {
                return new List<Post>();
            }

            var intersection = listsOfPosts[0];

            for (int i = 1; i < listsOfPosts.Count; i++)
            {
                var currentList = listsOfPosts[i];
                
                intersection = intersection
                    .Where(p => currentList.Any(x => x.Id == p.Id))
                    .ToList();
            }

            return intersection;
        }
    
        /* filter by filter object changes, above create a DB sql request per single filter, and also calculate intersection of sets which is expensive */
        
        public (List<Post> Posts, int TotalCount) GetFilteredPosts(PostFilter filter, string userRole)
        {
            var query = _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Owner)
                .AsQueryable();
            
            // 1. Apply status filter
            query = ApplyStatusFilter(query, filter.StatusFilter, userRole);
            
            // 2. Transaction type
            if (filter.IsForRent.HasValue)
            {
                query = query.Where(p => p.IsForRent == filter.IsForRent.Value);
            }
            
            // 3. Marques checklist
            if (filter.Constructors?.Any() == true)
            {
                query = query.Where(p => filter.Constructors.Contains(p.Constructor));
            }
            
            // 4. Modèles checklist
            if (filter.Models?.Any() == true)
            {
                query = query.Where(p => filter.Models.Contains(p.ModelName));
            }
            
            // 5. Price range
            if (filter.PriceMin.HasValue)
                query = query.Where(p => p.Price >= filter.PriceMin.Value);
            if (filter.PriceMax.HasValue)
                query = query.Where(p => p.Price <= filter.PriceMax.Value);
            
            // 6. Kilometrage range
            if (filter.KilometrageMin.HasValue)
                query = query.Where(p => p.Kilometrage >= filter.KilometrageMin.Value);
            if (filter.KilometrageMax.HasValue)
                query = query.Where(p => p.Kilometrage <= filter.KilometrageMax.Value);
            
            // 7. Year range
            if (filter.MinYear.HasValue)
                query = query.Where(p => p.PurchaseDate.Year >= filter.MinYear.Value);
            if (filter.MaxYear.HasValue)
                query = query.Where(p => p.PurchaseDate.Year <= filter.MaxYear.Value);
            
            // 8. Rating filter
            if (filter.RatingFilter == "4+")
                query = query.Where(p => p.RateAverage >= 4.0);
            else if (filter.RatingFilter == "5")
                query = query.Where(p => p.RateAverage == 5.0);
            
            // Get total count
            int totalCount = query.Count();
            
            // Apply pagination
            var posts = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * PostFilter.PageSize)
                .Take(PostFilter.PageSize)
                .ToList();
                
            return (posts, totalCount);
        }
        
        private IQueryable<Post> ApplyStatusFilter(IQueryable<Post> query, string? statusFilter, string userRole)
        {
            // Determine which statuses to show based on filter and role
            List<PostStatus> statuses;
            
            if (!string.IsNullOrEmpty(statusFilter))
            {
                // Use the filter from URL
                statuses = statusFilter switch
                {
                    "verified" => new List<PostStatus> { PostStatus.Accepted },
                    "pending" => new List<PostStatus> { PostStatus.Pending },
                    "refused" => new List<PostStatus> { PostStatus.Rejected },
                    "deleted" => new List<PostStatus> { PostStatus.Deleted },
                    "active" => new List<PostStatus> { PostStatus.Pending, PostStatus.Accepted, PostStatus.Rejected },
                    "all" => Enum.GetValues<PostStatus>().ToList(),
                    _ => GetDefaultStatuses(userRole) // Invalid filter, use default
                };
            }
            else
            {
                // No filter specified, use role-based default
                statuses = GetDefaultStatuses(userRole);
            }
            
            return query.Where(p => statuses.Contains(p.Status));
        }
        
        private List<PostStatus> GetDefaultStatuses(string userRole)
        {
            return userRole switch
            {
                "ADMIN" => new List<PostStatus> { PostStatus.Pending, PostStatus.Accepted, PostStatus.Rejected }, // active
                "EXPERT" => new List<PostStatus> { PostStatus.Pending },
                _ => new List<PostStatus> { PostStatus.Accepted } // USER
            };
        }
        
        // Get all unique marques for the filter checklist
        public List<string> GetAllConstructors()
        {
            return _context.Posts
                .Select(p => p.Constructor)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
        
        // Get all unique modèles (or filtered by marques)
        public List<string> GetModels(List<string>? constructors = null)
        {
            var query = _context.Posts.AsQueryable();
            
            if (constructors?.Any() == true)
            {
                query = query.Where(p => constructors.Contains(p.Constructor));
            }
            
            return query
                .Select(p => p.ModelName)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
        }
        
        // Get min/max values for range sliders
        public (decimal MinPrice, decimal MaxPrice, int MinKm, int MaxKm, int MinYear, int MaxYear) GetFilterRanges()
        {
            var posts = _context.Posts.AsQueryable();
            
            return (
                MinPrice: posts.Min(p => (decimal?)p.Price) ?? 0,
                MaxPrice: posts.Max(p => (decimal?)p.Price) ?? 1000000,
                MinKm: posts.Min(p => (int?)p.Kilometrage) ?? 0,
                MaxKm: posts.Max(p => (int?)p.Kilometrage) ?? 500000,
                MinYear: posts.Min(p => (int?)p.PurchaseDate.Year) ?? 1950,
                MaxYear: posts.Max(p => (int?)p.PurchaseDate.Year) ?? DateTime.Now.Year
            );
        }
    }
}