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
        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Owner) // Load the Seller info
                .Include(p => p.Comments) // Load Comments
                .Include(p => p.Ratings) // Load Ratings
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ==========================================================
        // 2. ADD POST
        // ==========================================================
        public async Task<bool> AddPostAsync(Post post)
        {
            try
            {
                // Set defaults if missing
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
                // We check if the post actually exists first
                if (!_context.Posts.Any(p => p.Id == post.Id))
                {
                    return false;
                }

                // This tells EF Core to look at the ID inside 'post' and update that record
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
        public async Task<bool> DeletePostAsync(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return false; // Post not found
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
        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)      // Load the Author of the comment
                .OrderByDescending(c => c.CreatedAt) // Newest comments first
                .ToListAsync();
        }

        // ==========================================================
        // 6. GET RATINGS FOR A POST
        // ==========================================================
        public async Task<List<RatingPost>> GetRatingsByPostIdAsync(int postId)
        {
            // Note: We use Set<RatingPost>() or your specific DbSet name (e.g. _context.PostRatings)
            return await _context.Set<RatingPost>() 
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
                .Where(p => p.Constructor.ToLower().Contains(keyword) || 
                            p.ModelName.ToLower().Contains(keyword) ||
                            p.Title.ToLower().Contains(keyword))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByConstructorAsync(string constructor)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Where(p => p.Constructor.ToLower().Contains(constructor.ToLower()))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByModelAsync(string model)
        {
            return await _context.Posts
                .Include(p => p.Owner)
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
                .Where(p => p.OwnerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Posts.Include(p => p.Owner).AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.OrderBy(p => p.Price).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByKilometrageRangeAsync(int? minKm, int? maxKm)
        {
            var query = _context.Posts.Include(p => p.Owner).AsQueryable();

            if (minKm.HasValue)
                query = query.Where(p => p.Kilometrage >= minKm.Value);

            if (maxKm.HasValue)
                query = query.Where(p => p.Kilometrage <= maxKm.Value);

            return await query.OrderBy(p => p.Kilometrage).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByYearRangeAsync(int? minYear, int? maxYear)
        {
            var query = _context.Posts.Include(p => p.Owner).AsQueryable();

            if (minYear.HasValue)
                query = query.Where(p => p.ReleaseYear >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(p => p.ReleaseYear <= maxYear.Value);

            return await query.OrderByDescending(p => p.ReleaseYear).ToListAsync();
        }

        public async Task<List<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ==========================================================
        // 9. SPECIAL FILTERS (JSON & State)
        // ==========================================================

        public async Task<List<Post>> GetPostsBySpecificationAsync(string specKey, string specValue)
        {
            // Note: This relies on EF Core translation for Dictionary/JSON. 
            // If using standard Npgsql JSONB mapping, this usually works in newer versions.
            // If it fails, we might need to fetch all and filter in memory (slower) or use raw SQL.
            return await _context.Posts
                .Include(p => p.Owner)
                .Where(p => p.Specifications[specKey] == specValue)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByStateAsync(PostStatus status)
        {
            return await _context.Posts
                .Include(p => p.Owner)
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
                .Where(p => p.Status == PostStatus.Accepted) // Usually we only show accepted posts on Home
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Post>> GetTopRatedPostsAsync(int count)
        {
            return await _context.Posts
                .Include(p => p.Owner)
                .Where(p => p.Status == PostStatus.Accepted)
                .OrderByDescending(p => p.RateAverage) // Uses the calculated column we added
                .Take(count)
                .ToListAsync();
        }
        
        
        // ==========================================================
        // 11. ROLE-BASED LOGIC (Validation & Owner)
        // ==========================================================

        public async Task<bool> ValidatePostAsync(int postId, PostStatus status, string expertId)
        {
            try
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post == null) return false;

                // Update the status
                post.Status = status;

                // OPTIONAL: If you add a 'ValidatedBy' field to your Post model later, save the expertId here:
                // post.ValidatedBy = expertId; 

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
            // If the input is empty or null, return nothing
            if (listsOfPosts == null || !listsOfPosts.Any())
            {
                return new List<Post>();
            }

            // Start with the first list
            var intersection = listsOfPosts[0];

            // Loop through the rest and keep only the common items
            // We compare based on ID to be safe
            for (int i = 1; i < listsOfPosts.Count; i++)
            {
                var currentList = listsOfPosts[i];
                
                // Keep only posts where the ID exists in the current list
                intersection = intersection
                    .Where(p => currentList.Any(x => x.Id == p.Id))
                    .ToList();
            }

            return intersection;
        }
        
        
    }
}