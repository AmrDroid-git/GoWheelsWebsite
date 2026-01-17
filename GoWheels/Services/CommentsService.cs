using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly GoWheelsDbContext _context;

        public CommentsService(GoWheelsDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // CORE OPERATIONS
        // ==========================================================

        public async Task<bool> AddCommentAsync(Comment comment)
        {
            try
            {
                comment.CreatedAt = DateTime.UtcNow; // Ensure date is set
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding comment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            try
            {
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating comment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null) return false;

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting comment: {ex.Message}");
                return false;
            }
        }

        // ==========================================================
        // RETRIEVAL METHODS
        // ==========================================================

        // 1. Get by ID
        public async Task<Comment?> GetCommentByIdAsync(Guid id)
        {
            return await _context.Comments
                .Include(c => c.User) // Load the Author
                .Include(c => c.Post) // Load the Post
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // 2. Search Body (Global Search)
        public async Task<List<Comment>> SearchCommentsBodyAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<Comment>();

            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Where(c => c.Body.ToLower().Contains(keyword.ToLower()))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // 3. Get by Post ID
        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User) // We need the user to show WHO commented
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // 4. Get by User ID
        public async Task<List<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post) // We need the post to show WHERE they commented
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // 5. Filter by Date Range
        public async Task<List<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}