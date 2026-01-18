using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface IPostsService
    {
        Task<Post?> GetPostByIdAsync(string id); //use it to print comments of post
        Task<bool> AddPostAsync(Post post);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(string id);
        
        //work for a specific post
        Task<List<Comment>> GetCommentsByPostIdAsync(string postId);
        Task<List<RatingPost>> GetRatingsByPostIdAsync(string postId);

        Task<List<Post>> SearchPostsAsync(string keyword);
        Task<List<Post>> GetPostsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice);
        Task<List<Post>> GetPostsByKilometrageRangeAsync(int? minKm, int? maxKm);
        Task<List<Post>> GetPostsByYearRangeAsync(int? minYear, int? maxYear);
        Task<List<Post>> GetPostsByConstructorAsync(string constructor);
        Task<List<Post>> GetPostsByModelAsync(string model);
        Task<List<Post>> GetPostsBySpecificationAsync(string specKey, string specValue);
        Task<List<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<List<Post>> GetRecentPostsAsync(int count);
        Task<List<Post>> GetTopRatedPostsAsync(int count);

        Task<List<Post>> GetPostsByStateAsync(PostStatus status);
        Task<bool> ValidatePostAsync(string postId, PostStatus status, string expertId);
        Task<List<Post>> GetPostsByOwnerAsync(string userId);
        
        
        // --- Utilities ---
        // Takes a list of lists (e.g., results from Price filter, Year filter, Model filter...)
        // Returns only the posts that exist in ALL of them.
        List<Post> IntersectPosts(List<List<Post>> listsOfPosts);
        
    }
}