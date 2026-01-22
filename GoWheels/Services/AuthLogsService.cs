using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GoWheels.Services
{
    public class AuthLogsService
    {
        private readonly IAdminLogsService _logsService;

        public AuthLogsService(IAdminLogsService logsService)
        {
            _logsService = logsService;
        }

        public async Task LogLoginAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            await _logsService.LogAsync(
                action: "USER_LOGIN",
                actorId: userId
            );
        }

        public async Task LogLogoutAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            await _logsService.LogAsync(
                action: "USER_LOGOUT",
                actorId: userId
            );
        }

        public async Task LogRegisterAsync(string userId)
        {
            await _logsService.LogAsync(
                action: "USER_REGISTERED",
                actorId: userId
            );
        }
    }
}