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

        // Adds a new comment to the database and sets the creation time
        public async Task<bool> AddCommentAsync(Comment comment)
        {
            try
            {
                comment.CreatedAt = DateTime.UtcNow; 
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

        // Updates the content of an existing comment
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

        // Deletes a comment by its ID (Interface asks for Guid, DB uses String, so we convert)
        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            try
            {
                // We convert the Guid to a String because your Model defines Id as a String
                var comment = await _context.Comments.FindAsync(id.ToString());
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

        // Retrieves a single comment by ID, including the Author, Post, and the Post's images
        public async Task<Comment?> GetCommentByIdAsync(string id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                    .ThenInclude(p => p.PostImages) 
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Searches for comments containing specific text, including Author and Post details with images
        public async Task<List<Comment>> SearchCommentsBodyAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<Comment>();

            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                    .ThenInclude(p => p.PostImages) 
                .Where(c => c.Body.ToLower().Contains(keyword.ToLower()))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Gets all comments for a specific post (Images NOT needed here as the car is already visible on the page)
        public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Gets all comments made by a specific user, including the Post thumbnail so they see what they commented on
        public async Task<List<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                    .ThenInclude(p => p.PostImages) 
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Filters comments within a specific date range, useful for admin reports
        public async Task<List<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                    .ThenInclude(p => p.PostImages) 
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}