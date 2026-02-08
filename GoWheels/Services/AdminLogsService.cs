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

        public async Task LogAsync(string? actorId, string action, string? details = null)
        {
            if (string.IsNullOrEmpty(actorId)) return;

            // Check if user exists to avoid FK violation (PostgresException: 23503)
            // This can happen if a user has a stale session cookie after a database reset
            var userExists = await _context.Users.AnyAsync(u => u.Id == actorId);
            if (!userExists) return;

            var log = new AdminLog
            {
                ActorId = actorId,
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