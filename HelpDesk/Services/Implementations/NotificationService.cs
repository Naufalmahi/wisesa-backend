using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(ApplicationDbContext context) { _context = context; }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly = false)
        {
            var query = _context.Notifications.Include(n => n.Ticket).Where(n => n.UserId == userId);
            if (unreadOnly) query = query.Where(n => !n.IsRead);
            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n == null) return false;
            n.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
