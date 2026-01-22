using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface IAdminLogsService
    {
        Task LogAsync(string actorId, string action, string? details = null);
        Task<List<AdminLog>> GetLogsAsync(int limit = 100);
    }
}