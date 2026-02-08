using Microsoft.AspNetCore.Mvc;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.ViewModels;
using GoWheels.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace GoWheels.Controllers
{
    public class UserController : Controller
    {
        private readonly GoWheelsDbContext _context;
        private readonly IUsersService _usersService;
        private readonly IRatingsService _ratingsService;
        private readonly IAdminLogsService _adminLogsService;


        public UserController(
            GoWheelsDbContext context,
            IUsersService usersService,
            IRatingsService ratingsService,
            IAdminLogsService adminLogsService)
        {
            _context = context;
            _usersService = usersService;
            _ratingsService = ratingsService;
            _adminLogsService = adminLogsService;
        }


        // GET: User
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllUsersAsync();
            //logs logic
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (actorId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "USERS_LIST_VIEWED",
                    actorId: actorId
                );
            }
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // If no ID provided, show current user's profile
                id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _usersService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if viewing own profile
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwnProfile = currentUserId == id;
            var canRate = User.Identity?.IsAuthenticated == true && !isOwnProfile && !User.IsInRole("ADMIN");

            // Fetch user's posts
            var userPosts = await _usersService.GetPostsByUserIdAsync(id);

            // Fetch user's ratings
            var userRatings = await _ratingsService.GetRatingsForUserAsync(id);

            // Pass data to view
            ViewData["UserPosts"] = userPosts;
            ViewData["UserRatings"] = userRatings;
            ViewData["IsOwnProfile"] = isOwnProfile;
            ViewData["CanRate"] = canRate;
            ViewData["CurrentUserId"] = currentUserId;

            // Logs logic
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (actorId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "USER_PROFILE_VIEWED",
                    actorId: actorId,
                    details: $"ViewedUserId={id}"
                );
            }

            return View(user);
        }


        // GET: User/EditContact
        [Authorize]
        public async Task<IActionResult> EditContact()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null) return NotFound();
            
            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address ?? ""
            };
            
            return View(viewModel);
        }

        // POST: User/EditContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditContact(UserEditViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);
            
            var user = await _context.Users.FindAsync(viewModel.Id);
            if (user == null) return NotFound();
            
            // Update fields
            user.Name = viewModel.Name;
            user.PhoneNumber = viewModel.PhoneNumber;
            user.Address = viewModel.Address;
            
            await _context.SaveChangesAsync();
            
            // Log the edit
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (actorId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "USER_PROFILE_EDITED",
                    actorId: actorId,
                    details: $"UserId={user.Id}"
                );
            }
            
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
    }
}
