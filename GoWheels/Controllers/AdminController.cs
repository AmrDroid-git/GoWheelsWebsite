using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GoWheels.Services.Interfaces;
using GoWheels.Models;

namespace GoWheels.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly IPostsService _postsService;
        private readonly IUsersService _usersService;
        private readonly IAdminLogsService _adminLogsService;
        // Inject the Service
        public AdminController(IPostsService postsService, IUsersService usersService,IAdminLogsService adminLogsService)
        {
            _postsService = postsService;
            _usersService = usersService;
            _adminLogsService = adminLogsService;
            
        }
        
        
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Logs
        public async Task<IActionResult> Logs()
        {
            // Récupère les derniers logs (par exemple 200)
            var logs = await _adminLogsService.GetLogsAsync(200);

            return View(logs);
        }


        // --- POSTS MANAGEMENT SECTION ---


        // GET: /Admin/PostsManagement
        public async Task<IActionResult> PostsManagement()
        {
            try
            {
                // Single query for all post counts
                var (pending, accepted, rejected, deleted) = await _postsService.GetPostCountsAsync();
                
                ViewData["PendingCount"] = pending;
                ViewData["AcceptedCount"] = accepted;
                ViewData["RejectedCount"] = rejected;
                ViewData["DeletedCount"] = deleted;
            }
            catch
            {
                ViewData["PendingCount"] = 0;
                ViewData["AcceptedCount"] = 0;
                ViewData["RejectedCount"] = 0;
                ViewData["DeletedCount"] = 0;
            }
            
            return View();
        }

        // GET: /Admin/PendingPosts
        public async Task<IActionResult> PendingPosts()
        {
            var posts = await _postsService.GetPostsByStateAsync(PostStatus.Pending);
            return View("Posts/PendingPosts", posts);
        }

        // GET: /Admin/AcceptedPosts
        public async Task<IActionResult> AcceptedPosts()
        {
            var posts = await _postsService.GetPostsByStateAsync(PostStatus.Accepted);
            return View("Posts/AcceptedPosts", posts);
        }

        // GET: /Admin/RejectedPosts
        public async Task<IActionResult> RejectedPosts()
        {
            var posts = await _postsService.GetPostsByStateAsync(PostStatus.Rejected);
            return View("Posts/RejectedPosts", posts);
        }

        // GET: /Admin/DeletedPosts
        public async Task<IActionResult> DeletedPosts()
        {
            var posts = await _postsService.GetPostsByStateAsync(PostStatus.Deleted);
            return View("Posts/DeletedPosts", posts);
        }
        
        // GET: /Admin/DetailsPost/5
        public async Task<IActionResult> DetailsPost(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postsService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            return View("Posts/DetailsPost", post);
        }
        
        
        // POST: /Admin/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(string id, PostStatus newStatus)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postsService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Admin has full power, so we don't restrict to 'Pending' only.
            post.Status = newStatus;

            var success = await _postsService.UpdatePostAsync(post);

            if (success)
            {
                TempData["SuccessMessage"] = $"Status changed to {newStatus} successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update post status.";
            }
            //logs logic
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (actorId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "POST_STATUS_CHANGED",
                    actorId: actorId,
                    details: $"PostId={post.Id}, NewStatus={newStatus}"
                );
            }


            // Stay on the same details page
            return RedirectToAction(nameof(DetailsPost), new { id = post.Id });
        }

        //USERS MANAGEMENT
        
        // GET: /Admin/UsersManagement
        public async Task<IActionResult> UsersManagement()
        {
            try
            {
                // Single query for all role counts
                var (userCount, expertCount, adminCount) = await _usersService.GetRoleCountsAsync();
                
                ViewData["UserCount"] = userCount;
                ViewData["ExpertCount"] = expertCount;
                ViewData["AdminCount"] = adminCount;
            }
            catch
            {
                ViewData["UserCount"] = 0;
                ViewData["ExpertCount"] = 0;
                ViewData["AdminCount"] = 0;
            }
            
            return View();
        }
        
        // GET: /Admin/UsersList (Regular Users)
        public async Task<IActionResult> UsersList()
        {
            var users = await _usersService.GetUsersByRoleAsync("USER");
            ViewData["CurrentRole"] = "User";
            return View("Users/UsersList", users);
        }

        // GET: /Admin/ExpertsList
        public async Task<IActionResult> ExpertsList()
        {
            var experts = await _usersService.GetUsersByRoleAsync("EXPERT");
            ViewData["CurrentRole"] = "EXPERT";
            return View("Users/ExpertsList", experts);
        }

        // GET: /Admin/AdminsList
        public async Task<IActionResult> AdminsList()
        {
            var admins = await _usersService.GetUsersByRoleAsync("ADMIN");
            ViewData["CurrentRole"] = "ADMIN";
            return View("Users/AdminsList", admins);
        }
    }
}