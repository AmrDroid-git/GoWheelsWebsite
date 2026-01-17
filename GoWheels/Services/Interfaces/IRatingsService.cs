using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface IRatingsService
    {
        // --- Core Operations ---
        // (You might want these for deleting/updating later)
        Task<bool> AddPostRatingAsync(RatingPost rating);
        Task<bool> AddUserRatingAsync(RatingUser rating);
        Task<bool> DeleteRatingAsync(int ratingId, bool isPostRating); // ID might duplicate across tables, so we need the type

        // --- Retrieval by ID ---
        Task<RatingPost?> GetPostRatingByIdAsync(int id);
        Task<RatingUser?> GetUserRatingByIdAsync(int id);

        // --- Retrieval by Context (Received Ratings) ---
        
        // 1. Get all ratings received by a specific Post
        Task<List<RatingPost>> GetRatingsForPostAsync(int postId);

        // 2. Get all ratings received by a specific User (Seller)
        Task<List<RatingUser>> GetRatingsForUserAsync(string userId);

        // --- Retrieval by Owner (Given Ratings) ---
        
        // 3. Get ALL ratings (both Post and User ratings) done by a specific user
        // Returns a polymorphic list of the base class 'Rating'
        Task<List<Rating>> GetAllRatingsGivenByUserAsync(string userId);
    }
}