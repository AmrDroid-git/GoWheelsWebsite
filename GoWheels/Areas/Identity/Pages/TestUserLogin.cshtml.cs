using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using GoWheels.Models;

namespace GoWheels.Pages
{
    public class TestUserLoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TestUserLoginModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string DisplayName { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // Read the 'Name' property from your custom model
                DisplayName = user.Name; 
            }
            else
            {
                DisplayName = "Guest";
            }
        }
    }
}