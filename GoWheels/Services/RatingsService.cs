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
                _context.PostsRatings.Add(rating);
                await _context.SaveChangesAsync();
                await UpdatePostRateAverageAsync(rating.RatedPostId);
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

        public async Task<bool> DeleteRatingAsync(int ratingId, bool isPostRating)
        {
            try
            {
                if (isPostRating)
                {
                    var r = await _context.PostsRatings.FindAsync(ratingId);
                    if (r == null) return false;

                    var postId = r.RatedPostId;

                    _context.PostsRatings.Remove(r);
                    await _context.SaveChangesAsync();

                    await UpdatePostRateAverageAsync(postId);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var r = await _context.UsersRatings.FindAsync(ratingId);
                    if (r == null) return false;

                    _context.UsersRatings.Remove(r);
                    await _context.SaveChangesAsync();
                }

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
        //pour la mise à jour du avg rating des posts à chaque ajout et supp des posts
        private async Task UpdatePostRateAverageAsync(string postId)
        {
            var average = await _context.PostsRatings
                .Where(r => r.RatedPostId == postId)
                .AverageAsync(r => (float?)r.Value);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return;

            post.RateAverage = average.HasValue
                ? (float)Math.Round(average.Value, 2, MidpointRounding.AwayFromZero)
                : null;
        }
        
        // pour la mise à jour du avg rating des posts lors du lancement du serveur
        public async Task RecalculateAllPostsRateAverageAsync()
        {
            var posts = await _context.Posts.ToListAsync();

            foreach (var post in posts)
            {
                var avg = await _context.PostsRatings
                    .Where(r => r.RatedPostId == post.Id)
                    .AverageAsync(r => (float?)r.Value);

                post.RateAverage = avg.HasValue
                    ? (float)Math.Round(avg.Value, 2, MidpointRounding.AwayFromZero)
                    : null;
            }

            await _context.SaveChangesAsync();
        }

    }
}