using HelpDesk.Models.Entities;

namespace HelpDesk.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task<ActivityLog> LogAsync(Guid ticketId, Guid userId, string action, string? oldValue = null, string? newValue = null);

        Task<IEnumerable<ActivityLog>> GetLogsByTicketIdAsync(Guid ticketId);

        Task<IEnumerable<ActivityLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 50);
    }
}