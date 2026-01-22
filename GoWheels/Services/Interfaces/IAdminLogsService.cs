using GoWheels.Models;

namespace GoWheels.Services.Interfaces
{
    public interface IAdminLogsService
    {
        //each log is created , we need to specify the id_user with admin roles
        Task LogAsync(string adminId,string userId, string action, string? details = null);
        Task<List<AdminLog>> GetLogsAsync(int limit = 100);
    }
}