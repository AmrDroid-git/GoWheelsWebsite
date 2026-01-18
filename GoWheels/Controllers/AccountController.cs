using Microsoft.AspNetCore.Mvc;
using GoWheels.Services.Interfaces;

namespace GoWheels.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;

        // Inject the service
        public AccountController(IUsersService usersService)
        {
            _usersService = usersService;
        }
        
        
        // GET: /login
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        
        
        // --- NEW: POST Login ---
        // This handles the form submission
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Basic validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            // 2. Call the Service
            bool success = await _usersService.LoginUserAsync(email, password);

            if (success)
            {
                // 3. Success -> Go to Home Page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // 4. Failure -> Stay on page and show error
                ViewBag.Error = "Invalid email or password.";
                return View();
            }
        }
        
        
        // GET: /register
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        
        
        // Logout Route
        [HttpPost("logout")] 
        public async Task<IActionResult> Logout()
        {
            await _usersService.LogoutUserAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}