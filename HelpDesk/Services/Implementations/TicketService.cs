using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLog;

        public TicketService(ApplicationDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);
                try
                {
                    ticket.TicketNumber = await GenerateTicketNumberAsync();
                    ticket.Status = TicketStatus.Open;
                    ticket.CreatedAt = DateTime.UtcNow;
                    ticket.UpdatedAt = DateTime.UtcNow;

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await _activityLog.LogAsync(ticket.Id, ticket.UserId, "Ticket Created",
                        null, $"Ticket {ticket.TicketNumber} dibuat dengan prioritas {ticket.Priority}");
                    return ticket;
                }
                catch (DbUpdateException) when (attempt < maxRetries - 1)
                {
                    await transaction.RollbackAsync();
                    _context.Entry(ticket).State = EntityState.Detached;
                    ticket.Id = Guid.NewGuid(); 
                    await Task.Delay(50 * (attempt + 1));
                }
            }

            throw new InvalidOperationException("Gagal membuat ticket setelah beberapa percobaan. Silakan coba lagi.");
        }

        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Tickets
                .Include(t => t.Creator).Include(t => t.Assignee).Include(t => t.Category)
                .Include(t => t.Comments.Where(c => c.DeletedAt == null)).ThenInclude(c => c.User)
                .Include(t => t.TicketSlas)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber)
            => await _context.Tickets.Include(t => t.Creator).Include(t => t.Assignee).Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

        public async Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId)
            => await _context.Tickets.Include(t => t.Assignee).Include(t => t.Category)
                .Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByAssigneeIdAsync(Guid assigneeId)
            => await _context.Tickets.Include(t => t.Creator).Include(t => t.Category).Include(t => t.TicketSlas)
                .Where(t => t.AssignedToId == assigneeId).OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetAllAsync(TicketStatus? status = null, TicketPriority? priority = null, Guid? categoryId = null)
        {
            var q = _context.Tickets.Include(t => t.Creator).Include(t => t.Assignee).Include(t => t.Category).AsQueryable();
            if (status.HasValue) q = q.Where(t => t.Status == status.Value);
            if (priority.HasValue) q = q.Where(t => t.Priority == priority.Value);
            if (categoryId.HasValue) q = q.Where(t => t.CategoryId == categoryId.Value);
            return await q.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid ticketId, TicketStatus newStatus, Guid changedByUserId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var oldStatus = ticket.Status;
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            if (newStatus == TicketStatus.Closed) ticket.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _activityLog.LogAsync(ticketId, changedByUserId, "Status Changed", oldStatus.ToString(), newStatus.ToString());
            return true;
        }

        public async Task<bool> AssignTicketAsync(Guid ticketId, Guid technicianId, Guid assignedByUserId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var oldAssignee = ticket.AssignedToId?.ToString() ?? "Unassigned";
            ticket.AssignedToId = technicianId;
            ticket.UpdatedAt = DateTime.UtcNow;
            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.Reopened)
                ticket.Status = TicketStatus.InProgress;

            await _context.SaveChangesAsync();
            await _activityLog.LogAsync(ticketId, assignedByUserId, "Ticket Assigned", oldAssignee, technicianId.ToString());
            return true;
        }

        public async Task<bool> ResolveTicketAsync(Guid ticketId, Guid resolvedByUserId)
            => await UpdateStatusAsync(ticketId, TicketStatus.Resolved, resolvedByUserId);

        public async Task<bool> CloseTicketAsync(Guid ticketId, Guid closedByUserId)
            => await UpdateStatusAsync(ticketId, TicketStatus.Closed, closedByUserId);

        public async Task<bool> ReopenTicketAsync(Guid ticketId, Guid reopenedByUserId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;
            if (ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed) return false;
            ticket.ClosedAt = null;
            return await UpdateStatusAsync(ticketId, TicketStatus.Reopened, reopenedByUserId);
        }

        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            var existing = await _context.Tickets.FindAsync(ticket.Id);
            if (existing == null) return false;
            existing.Title = ticket.Title;
            existing.Description = ticket.Description;
            existing.Priority = ticket.Priority;
            existing.CategoryId = ticket.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<TicketStatus, int>> GetTicketCountByStatusAsync()
            => await _context.Tickets.GroupBy(t => t.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

        public async Task<IEnumerable<Ticket>> GetAutoCloseableTicketsAsync()
        {
            var threshold = DateTime.UtcNow.AddHours(-24);
            return await _context.Tickets
                .Where(t => t.Status == TicketStatus.Resolved && t.UpdatedAt <= threshold)
                .ToListAsync();
        }
        private async Task<string> GenerateTicketNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"HD-{year}-";
            var lastTicket = await _context.Tickets
                .Where(t => t.TicketNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();

            int next = 1;
            if (lastTicket != null)
            {
                var numStr = lastTicket.TicketNumber.Substring(prefix.Length);
                if (int.TryParse(numStr, out int last)) next = last + 1;
            }
            return $"{prefix}{next:D5}";
        }
    }
}
