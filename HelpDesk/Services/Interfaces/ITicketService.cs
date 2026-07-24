using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;

namespace HelpDesk.Services.Interfaces
{
    public interface ITicketService
    {
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket?> GetByIdAsync(Guid id);
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Ticket>> GetByAssigneeIdAsync(Guid assigneeId);
        Task<IEnumerable<Ticket>> GetAllAsync(TicketStatus? status = null, TicketPriority? priority = null, Guid? categoryId = null);
        Task<bool> UpdateStatusAsync(Guid ticketId, TicketStatus newStatus, Guid changedByUserId);
        Task<bool> AssignTicketAsync(Guid ticketId, Guid technicianId, Guid assignedByUserId);
        Task<bool> ResolveTicketAsync(Guid ticketId, Guid resolvedByUserId);
        Task<bool> CloseTicketAsync(Guid ticketId, Guid closedByUserId);
        Task<bool> ReopenTicketAsync(Guid ticketId, Guid reopenedByUserId);
        Task<bool> UpdateTicketAsync(Ticket ticket);
        Task<Dictionary<TicketStatus, int>> GetTicketCountByStatusAsync();
        Task<IEnumerable<Ticket>> GetAutoCloseableTicketsAsync();
    }
}
