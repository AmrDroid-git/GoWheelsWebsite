// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GoWheels.Models;
using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GoWheels.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IUsersService _usersService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IUsersService usersService, ILogger<LogoutModel> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }

        // 1. We use OnGet so it works when you simply type /logout in the browser
        public async Task<IActionResult> OnGet()
        {
            // 2. Perform the logout
            await _usersService.LogoutUserAsync();
            
            _logger.LogInformation("User logged out.");

            // 3. FORCE Redirect to the Login page
            return LocalRedirect("/login"); 
        }

        // We keep OnPost to handle any buttons that POST to logout, 
        // but we just forward them to the logic above.
        public async Task<IActionResult> OnPost()
        {
            return await OnGet();
        }
    }
}