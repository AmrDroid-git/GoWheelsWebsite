using Microsoft.AspNetCore.Mvc;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


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
        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllUsersAsync();
            //logs logic
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _adminLogsService.LogAsync(
                action: "USERS_LIST_VIEWED",
                actorId: actorId
            );
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _usersService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // We need to fetch the posts of this user
            var userPosts = await _usersService.GetPostsByUserIdAsync(id);
            ViewData["UserPosts"] = userPosts;

            // We might also want to show ratings or comments
            var userRatings = await _ratingsService.GetRatingsForUserAsync(id);
            ViewData["UserRatings"] = userRatings;
            //Logs logic
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _adminLogsService.LogAsync(
                action: "USER_PROFILE_VIEWED",
                actorId: actorId,
                details: $"ViewedUserId={id}"
            );
            return View(user);
        }

        // Action to handle rating a user could be added here or in a separate controller
        // For now, we just show the details where the rating UI will reside.
    }
}
