// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using GoWheels.Models;
using GoWheels.Services.Interfaces;
using GoWheels.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace GoWheels.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUsersService _usersService;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AuthLogsService _authLogsService;

        public RegisterModel(
            SignInManager<ApplicationUser> signInManager,
            IUsersService usersService,
            ILogger<RegisterModel> logger,
            AuthLogsService authLogsService)
        {
            _signInManager = signInManager;
            _usersService = usersService;
            _logger = logger;
            _authLogsService = authLogsService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name / Username")]
            public string Name { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Full Address")]
            public string Address { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    // 1. UserName: Replaces spaces with underscores (e.g. "Amr Slama" -> "Amr_Slama")
                    UserName = Input.Name.Replace(" ", "_"),
                    
                    // 2. Name: Keeps the original name with spaces (e.g. "Amr Slama")
                    Name = Input.Name,
                    
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    
                    Address = Input.Address 
                };

                var (success, errorMessage) = await _usersService.CreateUserAsync(user, Input.Password);

                if (success)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _authLogsService.LogRegisterAsync(user.Id);//Logs logic
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }

            return Page();
        }
    }
}