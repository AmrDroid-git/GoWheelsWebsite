using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;
using GoWheels.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;



namespace GoWheels.Controllers
{
    public class PostController : Controller
    {
        private readonly GoWheelsDbContext _context;
        private readonly IPostsService _postsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAdminLogsService _adminLogsService;

        public PostController(
            GoWheelsDbContext context,
            IPostsService postsService,
            IWebHostEnvironment webHostEnvironment,
            IAdminLogsService adminLogsService)
        {
            _context = context;
            _postsService = postsService;
            _webHostEnvironment = webHostEnvironment;
            _adminLogsService = adminLogsService;
        }

        /* non filterable 
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var recentPosts = await _postsService.GetRecentPostsAsync(6);
            // var recentPosts = await _context.Posts.ToListAsync();
            return View(recentPosts);
        }*/

        [Authorize(Roles = "ADMIN")]
        public IActionResult TestDb()
        {
            var totalPosts = _context.Posts.Count();
            var byStatus = _context.Posts
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return Json(new { totalPosts, byStatus });
        }

        private string GetUserRole()
        {
            if (User.IsInRole("ADMIN")) return "ADMIN";
            if (User.IsInRole("EXPERT")) return "EXPERT";
            return "USER";
        }

        // Replace your existing Index method with this:
        public IActionResult Index(PostFilter? filter = null)
        {
            filter ??= new PostFilter();
            var userRole = GetUserRole();

            // Get filtered posts
            var (posts, totalCount) = _postsService.GetFilteredPosts(filter, userRole);

            // Get filter boundaries
            var filterRanges = _postsService.GetFilterRanges();
            var allConstructors = _postsService.GetAllConstructors();
            var models = _postsService.GetModels(filter.Constructors);

            // Calculate pagination
            int totalPages = (int)Math.Ceiling(totalCount / (double)PostFilter.PageSize);

            // Pass data to view
            ViewBag.Posts = posts;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = filter.Page;
            ViewBag.TotalPages = totalPages;

            // Current filter values
            ViewBag.CurrentFilter = filter;

            // Filter options
            ViewBag.Constructors = allConstructors;
            ViewBag.Models = models;
            ViewBag.FilterRanges = filterRanges;
            ViewBag.UserRole = userRole;

            // Status options for current role
            ViewBag.StatusOptions = GetStatusOptions(userRole);

            return View();
        }

        // AJAX endpoint for marque->modèle dependency
        [HttpPost]
        public IActionResult GetModels([FromBody] List<string>? constructors)
        {
            var models = _postsService.GetModels(constructors);
            return Json(models);
        }

        private List<SelectListItem> GetStatusOptions(string userRole)
        {
            var options = new List<SelectListItem>();

            switch (userRole)
            {
                case "ADMIN":
                    options.AddRange(new[]
                    {
                        new SelectListItem("Verified", "verified"),
                        new SelectListItem("Pending", "pending"),
                        new SelectListItem("Refused", "refused"),
                        new SelectListItem("Deleted", "deleted"),
                        new SelectListItem("(non-deleted)", "active"),
                        new SelectListItem("All", "all")
                    });
                    break;

                case "EXPERT":
                    options.AddRange(new[]
                    {
                        new SelectListItem("Pending", "pending"),
                        new SelectListItem("Verified", "verified"),
                        new SelectListItem("Refused", "refused"),
                        new SelectListItem("All", "active")
                    });
                    break;

                default: // USER
                    options.AddRange(new[]
                    {
                        new SelectListItem("Verified", "verified"),
                        new SelectListItem("Pending", "pending"),
                        new SelectListItem("Refused", "refused"),
                        new SelectListItem("All", "active")
                    });
                    break;
            }

            return options;
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.PostImages)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/MyPosts
        [Authorize] // Both USER and EXPERT can see their own posts
        public async Task<IActionResult> MyPosts()
        {
            // Get current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Shouldn't happen due to [Authorize] but just in case
            }

            // Get ALL user's posts (no pagination, no filtering by other statuses, but exclude Deleted)
            var userPosts = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Owner)
                .Where(p => p.OwnerId == userId && p.Status != PostStatus.Deleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Pass posts directly to view
            return View(userPosts);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(PostCreateViewModel viewmodel)
        {
            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                // DEBUG: Show validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View(viewmodel);
            }

            // Validating Constraints
            if (viewmodel.Images == null || viewmodel.Images.Count == 0)
            {
                ModelState.AddModelError(nameof(viewmodel.Images), "Please upload at least an image.");
                return View(viewmodel);
            }

            // Process Specifications into Dictionary
            var specifications = new Dictionary<string, string>();
            if (viewmodel.SpecificationKeys != null && viewmodel.SpecificationValues != null)
            {
                for (int i = 0; i < Math.Min(viewmodel.SpecificationKeys.Count, viewmodel.SpecificationValues.Count); i++)
                {
                    if (!string.IsNullOrWhiteSpace(viewmodel.SpecificationKeys[i]) &&
                        !string.IsNullOrWhiteSpace(viewmodel.SpecificationValues[i]))
                    {
                        specifications[viewmodel.SpecificationKeys[i]] = viewmodel.SpecificationValues[i];
                    }
                }
            }

            // Adding Post to DB 
            var post = new Post
            {
                Constructor = viewmodel.Constructor,
                ModelName = viewmodel.ModelName,
                ReleaseDate = viewmodel.ReleaseDate,
                PurchaseDate = viewmodel.PurchaseDate,
                Kilometrage = viewmodel.Kilometrage,
                Price = viewmodel.Price,
                IsForRent = viewmodel.IsForRent,
                Specifications = specifications,
                OwnerId = userId,
                Status = PostStatus.Pending
            };

            if (!await _postsService.AddPostAsync(post))
            {
                ModelState.AddModelError(nameof(Post), "You can't create this post. Please reach out for more information.");
                return View(viewmodel);
            }

            // Logs logic - MOVE THIS BEFORE image processing
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (actorId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "POST_CREATED",
                    actorId: actorId,
                    details: $"PostId={post.Id}, OwnerId={post.OwnerId}"
                );
            }

            // Processing Images
            try
            {
                foreach (var iFormFile in viewmodel.Images)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(iFormFile.FileName);
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                    await using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await iFormFile.CopyToAsync(stream);
                    }

                    // Creating a PostImage entity
                    var postImage = new PostImage
                    {
                        ImageUrl = "/images/" + uniqueFileName,
                        PostId = post.Id
                    };
                    _context.PostImages.Add(postImage);
                }

                await _context.SaveChangesAsync(); // Save all images at once
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"Image processing error: {ex.Message}");

                ModelState.AddModelError(nameof(viewmodel.Images), "Some error with the uploaded images.");
                return View(viewmodel);
            }

            // Success - redirect to Index
            return RedirectToAction(nameof(Index));
        }

        /*        // GET: Posts/Edit/5
                public async Task<IActionResult> Edit(string id)
                {
                    if (id == null)
                    {
                        return NotFound();
                    }

                    var post = await _postsService.GetPostByIdAsync(id); // --> returns bool
                    if (post == null)
                    {
                        return NotFound();
                    }
                    ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", post.OwnerId);
                    return View(post);
                }

                // POST: Posts/Edit/5
                // To protect from overposting attacks, enable the specific properties you want to bind to.
                // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
                [HttpPost]
                [ValidateAntiForgeryToken]
                [Authorize(Roles = "USER,ADMIN")]
                public async Task<IActionResult> Edit(string id, [Bind("Id,CreatedAt,Status,RateAverage,Constructor,ModelName,ReleaseDate,PurchaseDate,Kilometrage,Price,Specifications,OwnerId")] Post post)
                {
                    if (id != post.Id)
                    {
                        return NotFound();
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(post);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!PostExists(post.Id))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        //logs logic
                        var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                        await _adminLogsService.LogAsync(
                            action: "POST_EDITED",
                            actorId: actorId,
                            details: $"PostId={post.Id}"
                        );
                        return RedirectToAction(nameof(Index));
                    }
                    ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", post.OwnerId);
                    return View(post);
                }
        */

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("ADMIN");
            var isOwner = userId == post.OwnerId;

            if (!isAdmin && !isOwner) return Forbid();

            // Non-admins cannot edit deleted posts (they are irreversible)
            if (!isAdmin && post.Status == PostStatus.Deleted)
            {
                return RedirectToAction(nameof(Details), new { id = post.Id });
            }

            // Map to ViewModel
            var viewModel = new PostEditViewModel
            {
                Id = post.Id,
                Constructor = post.Constructor,
                ModelName = post.ModelName,
                ReleaseDate = post.ReleaseDate,
                PurchaseDate = post.PurchaseDate,
                Kilometrage = post.Kilometrage,
                Price = post.Price,
                Status = post.Status,
                IsForRent = post.IsForRent,
                OwnerName = post.Owner?.Name ?? "Unknown",
                CreatedAt = post.CreatedAt,
                RateAverage = post.RateAverage,
                RatingsCount = post.RatingsCount,
                ExistingImages = post.PostImages.Select(pi => new PostImageViewModel
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl
                }).ToList()
            };

            // Add specifications
            if (post.Specifications != null)
            {
                foreach (var spec in post.Specifications)
                {
                    viewModel.SpecificationKeys.Add(spec.Key);
                    viewModel.SpecificationValues.Add(spec.Value);
                }
            }

            ViewData["IsAdmin"] = isAdmin;
            ViewData["IsOwner"] = isOwner;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, PostEditViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var existingPost = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPost == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("ADMIN");
            var isOwner = userId == existingPost.OwnerId;

            if (!isAdmin && !isOwner) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewData["IsAdmin"] = isAdmin;
                ViewData["IsOwner"] = isOwner;
                return View(viewModel);
            }

            try
            {
                // Update basic fields
                existingPost.Constructor = viewModel.Constructor;
                existingPost.ModelName = viewModel.ModelName;
                existingPost.ReleaseDate = viewModel.ReleaseDate;
                existingPost.PurchaseDate = viewModel.PurchaseDate;
                existingPost.Kilometrage = viewModel.Kilometrage;
                existingPost.Price = viewModel.Price;

                // Handle Status
                if (isAdmin)
                {
                    existingPost.Status = viewModel.Status;
                }
                else
                {
                    // For non-admins, if they edited anything, reset to Pending 
                    // (unless it was already Deleted, but they shouldn't be able to edit Deleted anyway)
                    if (existingPost.Status != PostStatus.Deleted)
                    {
                        existingPost.Status = PostStatus.Pending;
                    }
                }

                // Handle IsForRent
                if (isAdmin)
                {
                    existingPost.IsForRent = viewModel.IsForRent;
                }

                // Update specifications
                var specifications = new Dictionary<string, string>();
                if (viewModel.SpecificationKeys != null && viewModel.SpecificationValues != null)
                {
                    for (int i = 0; i < Math.Min(viewModel.SpecificationKeys.Count, viewModel.SpecificationValues.Count); i++)
                    {
                        if (!string.IsNullOrWhiteSpace(viewModel.SpecificationKeys[i]) &&
                            !string.IsNullOrWhiteSpace(viewModel.SpecificationValues[i]))
                        {
                            specifications[viewModel.SpecificationKeys[i]] = viewModel.SpecificationValues[i];
                        }
                    }
                }
                existingPost.Specifications = specifications;
                
                // Handle image deletions
                if (viewModel.ImagesToDelete != null && viewModel.ImagesToDelete.Count > 0)
                {
                    var imagesToRemove = existingPost.PostImages
                        .Where(pi => viewModel.ImagesToDelete.Contains(pi.Id))
                        .ToList();

                    foreach (var img in imagesToRemove)
                    {
                        // Remove from file system
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        
                        // Remove from DB
                        _context.PostImages.Remove(img);
                    }
                }

                // Add new images
                if (viewModel.NewImages != null && viewModel.NewImages.Count > 0)
                {
                    foreach (var imageFile in viewModel.NewImages)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                        await using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        var postImage = new PostImage
                        {
                            ImageUrl = "/images/" + uniqueFileName,
                            PostId = existingPost.Id
                        };
                        _context.PostImages.Add(postImage);
                    }
                }

                await _context.SaveChangesAsync();

                var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (actorId != null)
                {
                    await _adminLogsService.LogAsync(
                        action: "POST_EDITED",
                        actorId: actorId,
                        details: $"PostId={existingPost.Id}"
                    );
                }

                return RedirectToAction(nameof(Details), new { id = existingPost.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id)) return NotFound();
                throw;
            }
        }


        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Check permissions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("ADMIN");
            var isOwner = userId == post.OwnerId;

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Delete/5 (Soft Delete for users)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var post = await _context.Posts
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.Id == id);
    
            if (post == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("ADMIN");
            var isOwner = userId == post.OwnerId;

            if (!isAdmin && !isOwner) return Forbid();

            // Soft delete: Change status to Deleted
            post.Status = PostStatus.Deleted;
            await _context.SaveChangesAsync();

            // Log
            if (userId != null)
            {
                await _adminLogsService.LogAsync(
                    action: "POST_DELETED",
                    actorId: userId,
                    details: $"PostId={id}, Vehicle={post.Constructor} {post.ModelName}"
                );
            }

            return RedirectToAction(nameof(MyPosts));
        }

        private bool PostExists(string id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
