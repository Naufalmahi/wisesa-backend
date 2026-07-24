using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityLog> LogAsync(Guid ticketId, Guid userId, string action,
            string? oldValue = null, string? newValue = null)
        {
            var log = new ActivityLog
            {
                TicketId = ticketId,
                UserId = userId,
                Action = action,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }
        public async Task<IEnumerable<ActivityLog>> GetLogsByTicketIdAsync(Guid ticketId)
        {
            return await _context.ActivityLogs
                .Include(a => a.User)
                .Where(a => a.TicketId == ticketId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ActivityLogs
                .Include(a => a.Ticket)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 50)
        {
            return await _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Ticket)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}