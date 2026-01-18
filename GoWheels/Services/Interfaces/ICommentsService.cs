using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface ICommentsService
    {
        // --- Core Operations (Add/Update/Delete) ---
        Task<bool> AddCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid id);

        // --- Retrieval Methods (Requested) ---
        
        // 1. Get a single comment by ID
        Task<Comment?> GetCommentByIdAsync(string id);

        // 2. Search all comments containing a specific text
        Task<List<Comment>> SearchCommentsBodyAsync(string keyword);

        // 3. Get all comments for a specific POST
        Task<List<Comment>> GetCommentsByPostIdAsync(string postId);

        // 4. Get all comments for a specific USER
        Task<List<Comment>> GetCommentsByUserIdAsync(string userId);

        // 5. Filter comments by Date Range
        Task<List<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}