using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class SlaService : ISlaService
    {
        private readonly ApplicationDbContext _context;

        public SlaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TicketSla?> CreateTicketSlaAsync(Guid ticketId, Guid technicianId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId)
                ?? throw new ArgumentException("Ticket tidak ditemukan");

            var policy = await _context.SlaPolicies.FirstOrDefaultAsync(p => p.Priority == ticket.Priority)
                ?? throw new InvalidOperationException($"SLA Policy untuk prioritas {ticket.Priority} tidak ditemukan");

            var ticketSla = new TicketSla
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                TechnicianId = technicianId,
                SlaPolicyId = policy.Id,
                DeadlineAt = DateTime.UtcNow.AddMinutes(policy.ResolveMinutes),
                IsBreached = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketSlas.Add(ticketSla);
            await _context.SaveChangesAsync();
            return ticketSla;
        }

        public async Task<bool> CheckAndApplySlaBreachAsync(Guid ticketId)
        {
            var now = DateTime.UtcNow;

            var activeSla = await _context.TicketSlas
                .Include(ts => ts.Ticket)
                .FirstOrDefaultAsync(ts => ts.TicketId == ticketId && !ts.IsBreached);

            if (activeSla == null || activeSla.Ticket == null) return false;

            if (activeSla.Ticket.Status == TicketStatus.Resolved || activeSla.Ticket.Status == TicketStatus.Closed)
                return false;

            if (now > activeSla.DeadlineAt)
            {
                activeSla.IsBreached = true;
                activeSla.BreachedAt = now;

                _context.TicketSlas.Update(activeSla);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<TicketSla>> CheckAndMarkBreachedAsync()
        {
            var now = DateTime.UtcNow;

            var breached = await _context.TicketSlas
                .Include(ts => ts.Ticket)
                .Where(ts => !ts.IsBreached
                    && ts.DeadlineAt <= now
                    && ts.Ticket != null
                    && ts.Ticket.Status != TicketStatus.Resolved
                    && ts.Ticket.Status != TicketStatus.Closed)
                .ToListAsync();

            foreach (var sla in breached)
            {
                sla.IsBreached = true;
                sla.BreachedAt = now;
            }

            if (breached.Any())
                await _context.SaveChangesAsync();

            return breached;
        }

        public async Task<TicketSla?> GetActiveSlaByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketSlas
                .Include(ts => ts.SlaPolicy)
                .Include(ts => ts.Technician)
                .Where(ts => ts.TicketId == ticketId)
                .OrderByDescending(ts => ts.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TicketSla>> GetNearBreachAsync(int percentageThreshold = 75)
        {
            var now = DateTime.UtcNow;

            var active = await _context.TicketSlas
                .Include(ts => ts.Ticket)
                .Include(ts => ts.SlaPolicy)
                .Include(ts => ts.Technician)
                .Where(ts => !ts.IsBreached
                    && ts.Ticket != null
                    && ts.Ticket.Status != TicketStatus.Resolved
                    && ts.Ticket.Status != TicketStatus.Closed)
                .ToListAsync();

            return active.Where(sla =>
            {
                if (sla.SlaPolicy == null || sla.SlaPolicy.ResolveMinutes <= 0) return false;

                double total = sla.SlaPolicy.ResolveMinutes;
                double elapsed = (now - sla.DeadlineAt.AddMinutes(-total)).TotalMinutes;

                double currentPercentage = (elapsed / total) * 100;
                return currentPercentage >= percentageThreshold;
            });
        }
    }
}