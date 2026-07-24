using HelpDesk.Models.Entities;

namespace HelpDesk.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly = false);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
