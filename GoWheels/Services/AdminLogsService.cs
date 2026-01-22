using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services
{
    public class AdminLogsService : IAdminLogsService
    {
        private readonly GoWheelsDbContext _context;

        public AdminLogsService(GoWheelsDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action, string? details = null)
        {
            var log = new AdminLog
            {
               
                ActorId = userId,
                Action = action,
                Details = details
            };

            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdminLog>> GetLogsAsync(int limit = 100)
        {
            return await _context.AdminLogs
                .Include(l => l.Actor)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}