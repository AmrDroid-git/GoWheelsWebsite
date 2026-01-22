using Microsoft.AspNetCore.Mvc;
using GoWheels.Services.Interfaces;
using GoWheels.Models;

namespace GoWheels.Controllers
{
    public class AdminController : Controller
    {
        private readonly IPostsService _postsService;
        private readonly IUsersService _usersService;
        // Inject the Service
        public AdminController(IPostsService postsService, IUsersService usersService)
        {
            _postsService = postsService;
            _usersService = usersService;
        }
        
        
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Logs
        public IActionResult Logs()
        {
            return View();
        }

        // --- POSTS MANAGEMENT SECTION ---

        // GET: /Admin/PostsManagement
        public IActionResult PostsManagement()
        {
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

            // Stay on the same details page
            return RedirectToAction(nameof(DetailsPost), new { id = post.Id });
        }

        // POST: /Admin/DeletePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _postsService.DeletePostAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Post deleted from database permanently.";
                // Redirect to the main list since the post no longer exists
                return RedirectToAction(nameof(PostsManagement));
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete post.";
                return RedirectToAction(nameof(DetailsPost), new { id = id });
            }
        }
        
        
        //USERS MANAGEMENT
        
        // GET: /Admin/UsersManagement
        public IActionResult UsersManagement()
        {
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
            var experts = await _usersService.GetUsersByRoleAsync("Expert");
            ViewData["CurrentRole"] = "EXPERT";
            return View("Users/ExpertsList", experts);
        }

        // GET: /Admin/AdminsList
        public async Task<IActionResult> AdminsList()
        {
            var admins = await _usersService.GetUsersByRoleAsync("Admin");
            ViewData["CurrentRole"] = "ADMIN";
            return View("Users/AdminsList", admins);
        }
    }
}