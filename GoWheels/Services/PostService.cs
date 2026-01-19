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
    }
}