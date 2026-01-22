using Microsoft.AspNetCore.Mvc;
using GoWheels.Services.Interfaces;
using GoWheels.Models;
using System.Security.Claims;

namespace GoWheels.Controllers
{
    public class ExpertController : Controller
    {
        private readonly IPostsService _postsService;
        private readonly IAdminLogsService _adminLogsService;

        public ExpertController(
            IPostsService postsService,
            IAdminLogsService adminLogsService)
        {
            _postsService = postsService;
            _adminLogsService = adminLogsService;
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

            // ---CHECK: Ensure we only verify Pending posts ---
            if (post.Status != PostStatus.Pending)
            {
                TempData["ErrorMessage"] = $"This post has status {post.Status}, you can't verify it.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Change the status
            post.Status = newStatus;

            // 3. Update in Database using your Service
            var success = await _postsService.UpdatePostAsync(post);

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to update post status.";
            }
            else
            {
                var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _adminLogsService.LogAsync(
                    action: "EXPERT_POST_STATUS_CHANGED",
                    actorId: actorId,
                    details: $"PostId={post.Id}, NewStatus={newStatus}"
                );
            }

            // 4. Redirect back to the Expert Dashboard
            return RedirectToAction(nameof(Index));
        }
    }
}