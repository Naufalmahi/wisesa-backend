using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class EscalationService : IEscalationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketService _ticketService;
        private readonly IActivityLogService _activityLog;

        public EscalationService(ApplicationDbContext context, ITicketService ticketService, IActivityLogService activityLog)
        { _context = context; _ticketService = ticketService; _activityLog = activityLog; }

        public async Task<Escalation> CreateAsync(Guid ticketId, Guid fromUserId,
            EscalationLevel fromLevel, EscalationLevel toLevel, string reason)
        {
            var escalation = new Escalation
            {
                TicketId = ticketId, FromUserId = fromUserId,
                FromLevel = fromLevel, ToLevel = toLevel,
                Reason = reason, Status = EscalationStatus.Pending
            };
            _context.Escalations.Add(escalation);

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null) { ticket.Status = TicketStatus.Escalated; ticket.UpdatedAt = DateTime.UtcNow; }

            await _context.SaveChangesAsync();
            await _activityLog.LogAsync(ticketId, fromUserId, "Escalated",
                fromLevel.ToString(), $"{toLevel} - {reason}");
            return escalation;
        }

        public async Task<bool> ApproveAsync(Guid escalationId, Guid toUserId, Guid approvedByUserId)
        {
            var esc = await _context.Escalations.FindAsync(escalationId);
            if (esc == null || esc.Status != EscalationStatus.Pending) return false;

            esc.Status = EscalationStatus.Approved;
            esc.ToUserId = toUserId;
            esc.ResolvedAt = DateTime.UtcNow;

            // Re-assign ticket ke teknisi baru
            await _ticketService.AssignTicketAsync(esc.TicketId, toUserId, approvedByUserId);
            await _context.SaveChangesAsync();
            await _activityLog.LogAsync(esc.TicketId, approvedByUserId, "Escalation Approved",
                esc.FromLevel.ToString(), esc.ToLevel.ToString());
            return true;
        }

        public async Task<bool> RejectAsync(Guid escalationId, Guid rejectedByUserId)
        {
            var esc = await _context.Escalations.FindAsync(escalationId);
            if (esc == null || esc.Status != EscalationStatus.Pending) return false;

            esc.Status = EscalationStatus.Rejected;
            esc.ResolvedAt = DateTime.UtcNow;

            // Kembalikan status ticket ke InProgress
            await _ticketService.UpdateStatusAsync(esc.TicketId, TicketStatus.InProgress, rejectedByUserId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Escalation>> GetByTicketIdAsync(Guid ticketId)
            => await _context.Escalations.Include(e => e.FromUser).Include(e => e.ToUser)
                .Where(e => e.TicketId == ticketId).OrderByDescending(e => e.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Escalation>> GetPendingAsync()
            => await _context.Escalations.Include(e => e.Ticket).Include(e => e.FromUser)
                .Where(e => e.Status == EscalationStatus.Pending).OrderBy(e => e.CreatedAt).ToListAsync();
    }
}
