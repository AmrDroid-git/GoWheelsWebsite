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
                // We must know WHICH table to delete from
                if (isPostRating)
                {
                    var r = await _context.PostsRatings.FindAsync(ratingId);
                    if (r == null) return false;
                    _context.PostsRatings.Remove(r);
                }
                else
                {
                    var r = await _context.UsersRatings.FindAsync(ratingId);
                    if (r == null) return false;
                    _context.UsersRatings.Remove(r);
                }

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
        // GET BY ID
        // ==========================================================

        public async Task<RatingPost?> GetPostRatingByIdAsync(string id)
        {
            return await _context.PostsRatings
                .Include(r => r.Owner)
                .Include(r => r.RatedPost)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RatingUser?> GetUserRatingByIdAsync(string id)
        {
            return await _context.UsersRatings
                .Include(r => r.Owner)
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ==========================================================
        // GET RECEIVED RATINGS (Target Specific)
        // ==========================================================

        public async Task<List<RatingPost>> GetRatingsForPostAsync(string postId)
        {
            return await _context.PostsRatings
                .Where(r => r.RatedPostId == postId)
                .Include(r => r.Owner) // To show WHO rated
                .OrderByDescending(r => r.Id) // Newest first (using ID as proxy for time if no CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RatingUser>> GetRatingsForUserAsync(string userId)
        {
            return await _context.UsersRatings
                .Where(r => r.RatedUserId == userId)
                .Include(r => r.Owner) // To show WHO rated
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        // ==========================================================
        // GET GIVEN RATINGS (User History)
        // ==========================================================

        public async Task<List<Rating>> GetAllRatingsGivenByUserAsync(string userId)
        {
            // 1. Get Post Ratings given by this user
            var postRatings = await _context.PostsRatings
                .Where(r => r.OwnerId == userId)
                .Include(r => r.RatedPost) // Include what they rated
                .ToListAsync();

            // 2. Get User Ratings given by this user
            var userRatings = await _context.UsersRatings
                .Where(r => r.OwnerId == userId)
                .Include(r => r.RatedUser) // Include whom they rated
                .ToListAsync();

            // 3. Combine them into one polymorphic list
            // We cast both to the base 'Rating' class
            var allRatings = new List<Rating>();
            allRatings.AddRange(postRatings);
            allRatings.AddRange(userRatings);

            // Optional: Sort combined list by ID (most recent)
            return allRatings.OrderByDescending(r => r.Id).ToList();
        }
    }
}