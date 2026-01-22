using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;
using GoWheels.ViewModels;

namespace GoWheels.Controllers
{
    public class PostController : Controller
    {
        private readonly GoWheelsDbContext _context;
        private readonly IPostsService _postsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(GoWheelsDbContext context, IPostsService postsService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _postsService = postsService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var recentPosts = await _postsService.GetRecentPostsAsync(6);
            // var recentPosts = await _context.Posts.ToListAsync();
            return View(recentPosts);
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

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel viewmodel)
        {
            if (!ModelState.IsValid)
            {
                goto Repeat_Please;
            }
            
            // Validating Constraints
            if (viewmodel.Images == null || viewmodel.Images.Count == 0)
            {
                ModelState.AddModelError(nameof(viewmodel.Images), "Please upload at least an image.");
                goto Repeat_Please;
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
                OwnerId = viewmodel.OwnerId
            };
            if (! await _postsService.AddPostAsync(post))
            {
                ModelState.AddModelError(nameof(Post), "You can't create this post. Please reach out for more information.");
                goto Repeat_Please; // --> feature : show error
            }

            var postImages = new List<PostImage>();
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
                        stream.Close();
                    };
                    
                    // Creating a PostImage entity
                    var postImage = new PostImage
                    {
                        ImageUrl = "/images/" + uniqueFileName,
                        PostId = post.Id
                    };
                    _context.PostImages.Add(postImage);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                ModelState.AddModelError(nameof(viewmodel.Images), "Some error with the uploaded images.");
                goto Repeat_Please;
            }
            
            // Redirecting to Index with success
            // feature: show a green pop-up: "Successfully creating your post"
            return RedirectToAction(nameof(Index));
            
            // Some problem exists
            Repeat_Please :
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", viewmodel.OwnerId);
            return View(viewmodel);
        }

        // GET: Posts/Edit/5
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", post.OwnerId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(string id)
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

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(string id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
