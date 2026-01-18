using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly GoWheelsDbContext _context;

        public UsersService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            GoWheelsDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ==========================================================
        // 1. RETRIEVAL METHODS
        // ==========================================================

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        // ==========================================================
        // 2. ROLE-BASED RETRIEVAL
        // ==========================================================

        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            // Identity returns an IList, so we convert it to a standard List
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }

        // ==========================================================
        // 3. USER DATA RETRIEVAL (Comments)
        // ==========================================================

        public async Task<List<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post) // Include the Post so we know WHERE they commented
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Post>> GetPostsByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Where(p => p.OwnerId == userId)
                .Include(p => p.Owner) // Include Owner details (optional but good for display)
                .OrderByDescending(p => p.CreatedAt) // Show newest posts first
                .ToListAsync();
        }

        // ==========================================================
        // 4. MANAGEMENT & UPDATES
        // ==========================================================

        public async Task<bool> UpdateUserProfileAsync(ApplicationUser user)
        {
            try
            {
                // Verify user exists first
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser == null) return false;

                // Update fields
                existingUser.Name = user.Name;
                existingUser.Address = user.Address;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RateAverage = user.RateAverage; // Ensure rating is preserved/updated

                var result = await _userManager.UpdateAsync(existingUser);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user profile: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // 1. Check if the new role exists in the database
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                return false; 
            }

            // 2. Remove all existing roles (User should have only one primary role)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;
            }

            // 3. Add the new role
            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            return addResult.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        // ==========================================================
        // 5. RATING LOGIC
        // ==========================================================

        public async Task<bool> UpdateUserRatingAverageAsync(string userId)
        {
            try
            {
                // 1. Fetch all ratings received by this user
                // We use _context directly because UserManager doesn't handle custom tables like Ratings
                var ratings = await _context.UsersRatings
                    .Where(r => r.RatedUserId == userId)
                    .ToListAsync();

                // 2. Calculate Average
                float newAverage = 0f;
                if (ratings.Any())
                {
                    newAverage = ratings.Average(r => r.Value);
                }

                // 3. Fetch the user to update
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                // 4. Update and Save
                user.RateAverage = newAverage;
                
                // We use UpdateAsync from UserManager to ensure Identity creates the SQL update correctly
                var result = await _userManager.UpdateAsync(user);
                
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user rating: {ex.Message}");
                return false;
            }
        }
    }
}