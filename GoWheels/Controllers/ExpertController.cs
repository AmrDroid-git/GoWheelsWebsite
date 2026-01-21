using Microsoft.AspNetCore.Mvc;
using GoWheels.Services.Interfaces;
using GoWheels.Models;

namespace GoWheels.Controllers
{
    public class ExpertController : Controller
    {
        private readonly IPostsService _postsService;

        // Inject the Service, NOT the DbContext
        public ExpertController(IPostsService postsService)
        {
            _postsService = postsService;
        }

        // GET: /Expert
        public async Task<IActionResult> Index()
        {
            var posts = await _postsService.GetPostsByStateAsync(PostStatus.Pending);

            return View(posts);
        }
        
        // GET: /Expert/DetailsPost/5
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

            return View(post);
        }
        
        
        // POST: /Expert/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(string id, PostStatus newStatus)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Get the existing post
            var post = await _postsService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // 2. Change the status
            post.Status = newStatus;

            // 3. Update in Database using your Service
            var success = await _postsService.UpdatePostAsync(post);

            if (!success)
            {
                // Optional: Add an error message if update failed
                TempData["ErrorMessage"] = "Failed to update post status.";
            }

            // 4. Redirect back to the Expert Dashboard
            return RedirectToAction(nameof(Index));
        }
    }
}