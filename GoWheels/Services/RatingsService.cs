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
                var user = await _context.Users.FindAsync(rating.RatedUserId);
                if (user != null)
                {
                    float currentSum = (user.RateAverage ?? 0) * user.RatingsCount;
                    user.RatingsCount++;
                    user.RateAverage = (float)Math.Round((currentSum + rating.Value) / user.RatingsCount, 2, MidpointRounding.AwayFromZero);
                }

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

        public async Task<bool> SaveRatingPostAsync(RatingPost rating)
        {
            try
            {
                var existingRating = await _context.PostsRatings
                    .FirstOrDefaultAsync(r => r.RatedPostId == rating.RatedPostId && r.OwnerId == rating.OwnerId);

                if (existingRating != null)
                {
                    existingRating.Value = rating.Value;
                    _context.PostsRatings.Update(existingRating);
                }
                else
                {
                    _context.PostsRatings.Add(rating);
                }

                await _context.SaveChangesAsync();
                await RecalculatePostAverage(rating.RatedPostId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving post rating: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveRatingUserAsync(RatingUser rating)
        {
            try
            {
                var existingRating = await _context.UsersRatings
                    .FirstOrDefaultAsync(r => r.RatedUserId == rating.RatedUserId && r.OwnerId == rating.OwnerId);

                if (existingRating != null)
                {
                    existingRating.Value = rating.Value;
                    _context.UsersRatings.Update(existingRating);
                }
                else
                {
                    _context.UsersRatings.Add(rating);
                }

                await _context.SaveChangesAsync();
                await RecalculateUserAverage(rating.RatedUserId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user rating: {ex.Message}");
                return false;
            }
        }

        private async Task RecalculatePostAverage(string postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return;

            var ratings = await _context.PostsRatings.Where(r => r.RatedPostId == postId).ToListAsync();
            if (ratings.Any())
            {
                post.RatingsCount = ratings.Count;
                post.RateAverage = (float)Math.Round(ratings.Average(r => r.Value), 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                post.RatingsCount = 0;
                post.RateAverage = null;
            }
            await _context.SaveChangesAsync();
        }

        private async Task RecalculateUserAverage(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var ratings = await _context.UsersRatings.Where(r => r.RatedUserId == userId).ToListAsync();
            if (ratings.Any())
            {
                user.RatingsCount = ratings.Count;
                user.RateAverage = (float)Math.Round(ratings.Average(r => r.Value), 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                user.RatingsCount = 0;
                user.RateAverage = null;
            }
            await _context.SaveChangesAsync();
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
                var userId = r.RatedUserId;
                var ratingValue = r.Value;

                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    if (user.RatingsCount > 1)
                    {
                        float totalSum = (user.RateAverage ?? 0) * user.RatingsCount;
                        user.RatingsCount--;
                        user.RateAverage = (float)Math.Round((totalSum - ratingValue) / user.RatingsCount, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        user.RatingsCount = 0;
                        user.RateAverage = null;
                    }
                }

                _context.UsersRatings.Remove(r);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user rating: {ex.Message}");
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
        
        public async Task RecalculateAllUsersRateAverageAsync()
        {
            var stats = await _context.UsersRatings
                .GroupBy(r => r.RatedUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Average = g.Average(r => r.Value),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.UserId);

            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                if (stats.TryGetValue(user.Id, out var stat))
                {
                    user.RateAverage = (float)Math.Round(stat.Average, 2, MidpointRounding.AwayFromZero);
                    user.RatingsCount = stat.Count;
                }
                else
                {
                    user.RateAverage = null;
                    user.RatingsCount = 0;
                }
            }

            _context.Users.UpdateRange(users);
            await _context.SaveChangesAsync();
        }
    }
}