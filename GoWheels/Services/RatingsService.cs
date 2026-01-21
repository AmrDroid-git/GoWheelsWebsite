using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services
{
    public class RatingsService : IRatingsService
    {
        private readonly GoWheelsDbContext _context;

        public RatingsService(GoWheelsDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // ADD / DELETE OPERATIONS
        // ==========================================================

        public async Task<bool> AddPostRatingAsync(RatingPost rating)
        {
            try
            {
                var post = await _context.Posts.FindAsync(rating.RatedPostId);
                if (post != null)
                {
                    float currentSum = (post.RateAverage ?? 0) * post.RatingsCount;
                    post.RatingsCount++;
                    post.RateAverage = (float)Math.Round((currentSum + rating.Value) / post.RatingsCount, 2, MidpointRounding.AwayFromZero);
                }

                _context.PostsRatings.Add(rating);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post rating: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserRatingAsync(RatingUser rating)
        {
            try
            {
                _context.UsersRatings.Add(rating);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user rating: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePostRatingAsync(string id)
        {
            try
            {
                var r = await _context.PostsRatings.FindAsync(id);
                if (r == null) return false;
                var postId = r.RatedPostId;
                var ratingValue = r.Value;
                
                var post = await _context.Posts.FindAsync(postId);
                if (post != null)
                {
                    if (post.RatingsCount > 1)
                    {
                        float totalSum = (post.RateAverage ?? 0) * post.RatingsCount;
                        post.RatingsCount--;
                        post.RateAverage = (float)Math.Round((totalSum - ratingValue) / post.RatingsCount, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        post.RatingsCount = 0;
                        post.RateAverage = null;
                    }
                }

                _context.PostsRatings.Remove(r);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting rating: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteUserRatingAsync(string id)
        {
            try
            {
                var r = await _context.UsersRatings.FindAsync(id);
                if (r == null) return false;
                _context.UsersRatings.Remove(r);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting rating: {ex.Message}");
                return false;
            }
        }

        // ==========================================================
        // RETRIEVAL BY ID
        // ==========================================================

        public async Task<RatingPost?> GetPostRatingByIdAsync(string id)
        {
            
            return await _context.PostsRatings
                .Include(r => r.Owner)
                .Include(r => r.RatedPost)
                .ThenInclude(p => p.PostImages) 
                .FirstOrDefaultAsync(r => r.Id == id); // Compares string == string
        }

        public async Task<RatingUser?> GetUserRatingByIdAsync(string id)
        {
            return await _context.UsersRatings
                .Include(r => r.Owner)
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ==========================================================
        // RETRIEVAL BY CONTEXT (Received Ratings)
        // ==========================================================

        // Get all ratings for a specific car (Images not needed, we are already on the car page)
        public async Task<List<RatingPost>> GetRatingsForPostAsync(string postId)
        {
            return await _context.PostsRatings
                .Where(r => r.RatedPostId == postId)
                .Include(r => r.Owner)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        // Get all ratings for a seller (User)
        public async Task<List<RatingUser>> GetRatingsForUserAsync(string userId)
        {
            return await _context.UsersRatings
                .Where(r => r.RatedUserId == userId)
                .Include(r => r.Owner)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        // ==========================================================
        // RETRIEVAL BY OWNER (Given Ratings - History)
        // ==========================================================

        /**
         * This function is not good at all, it mixes the PostRatings and UserRatings
         */
        public async Task<List<Rating>> GetAllRatingsGivenByUserAsync(string userId)
        {
            // 1. Get Post Ratings (With Car Images for the list)
            var postRatings = await _context.PostsRatings
                .Where(r => r.OwnerId == userId)
                .Include(r => r.RatedPost)
                    .ThenInclude(p => p.PostImages) // <--- CRITICAL: Show thumbnail
                .ToListAsync();

            // 2. Get User Ratings
            var userRatings = await _context.UsersRatings
                .Where(r => r.OwnerId == userId)
                .Include(r => r.RatedUser)
                .ToListAsync();

            // 3. Combine and Sort
            var allRatings = new List<Rating>();
            allRatings.AddRange(postRatings);
            allRatings.AddRange(userRatings);

            return allRatings.OrderByDescending(r => r.Id).ToList();
        }
        
        // pour la mise à jour du avg rating des posts lors du lancement du serveur
        public async Task RecalculateAllPostsRateAverageAsync()
        {
            var stats = await _context.PostsRatings
                .GroupBy(r => r.RatedPostId)
                .Select(g => new
                {
                    PostId = g.Key,
                    Average = g.Average(r => r.Value),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.PostId);

            var posts = await _context.Posts.ToListAsync();

            foreach (var post in posts)
            {
                if (stats.TryGetValue(post.Id, out var stat))
                {
                    post.RateAverage = (float)Math.Round(stat.Average, 2, MidpointRounding.AwayFromZero);
                    post.RatingsCount = stat.Count;
                }
                else
                {
                    post.RateAverage = null;
                    post.RatingsCount = 0;
                }
            }

            _context.Posts.UpdateRange(posts);
            await _context.SaveChangesAsync();
        }

    }
}