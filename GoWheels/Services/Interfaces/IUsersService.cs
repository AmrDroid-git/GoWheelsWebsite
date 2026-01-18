using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface IUsersService
    {
        // --- Retrieval Methods ---
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<List<ApplicationUser>> GetAllUsersAsync(); 
        
        // --- Role-Based Retrieval ---
        Task<List<ApplicationUser>> GetUsersByRoleAsync(string role);

        // --- User Data Retrieval ---
        // Get all comments written by this specific user
        Task<List<Comment>> GetCommentsByUserIdAsync(string userId);
        Task<List<Post>> GetPostsByUserIdAsync(string userId);

        // --- Management & Updates ---
        
        // 1. Update Profile Info: Use this when user changes Name, Address,new rate added, or Phone
        Task<bool> UpdateUserProfileAsync(ApplicationUser user);

        // 2. Update Role: Promote or Demote users (Admin/Expert/Normal)
        Task<bool> ChangeUserRoleAsync(string userId, string newRole);

        // 3. Delete: Remove a user from the system
        Task<bool> DeleteUserAsync(string userId);

        // --- Rating Logic ---
        // Call this method whenever a new rating is added to 'ReceivedRatings'.
        // It should calculate the average of all ReceivedRatings and save it to RateAverage.
        Task<bool> UpdateUserRatingAverageAsync(string userId);
        
        // --- Authentication ---
        // Returns true if login is successful (email & password match)
        Task<bool> LoginUserAsync(string email, string password);
        
        // Optional: Good practice to have a Logout method here too
        Task LogoutUserAsync();
    }
}