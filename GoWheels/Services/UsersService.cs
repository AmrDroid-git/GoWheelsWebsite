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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly GoWheelsDbContext _context;
        
        public UsersService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            GoWheelsDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }

        public async Task<List<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                    .ThenInclude(p => p.PostImages)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Post>> GetPostsByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Where(p => p.OwnerId == userId)
                .Include(p => p.Owner) 
                .Include(p => p.PostImages)
                .OrderByDescending(p => p.CreatedAt) 
                .ToListAsync();
        }

        public async Task<bool> UpdateUserProfileAsync(ApplicationUser user)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser == null) return false;

                existingUser.Name = user.Name;
                existingUser.Address = user.Address;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RateAverage = user.RateAverage;

                var result = await _userManager.UpdateAsync(existingUser);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                return false; 
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;
            }

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

        public async Task<bool> UpdateUserRatingAverageAsync(string userId)
        {
            try
            {
                var ratings = await _context.UsersRatings
                    .Where(r => r.RatedUserId == userId)
                    .ToListAsync();

                float newAverage = 0f;
                if (ratings.Any())
                {
                    newAverage = ratings.Average(r => r.Value);
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                user.RateAverage = newAverage;
                
                var result = await _userManager.UpdateAsync(user);
                
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LoginUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

            return result.Succeeded;
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}