using HelpDesk.Models.Entities;

namespace HelpDesk.Services.Interfaces
{
    public interface ISlaService
    {
        Task<TicketSla?> CreateTicketSlaAsync(Guid ticketId, Guid technicianId);
        Task<bool> CheckAndApplySlaBreachAsync(Guid ticketId);
        Task<TicketSla?> GetActiveSlaByTicketIdAsync(Guid ticketId);

        Task<IEnumerable<TicketSla>> CheckAndMarkBreachedAsync();
        Task<IEnumerable<TicketSla>> GetNearBreachAsync(int percentage);
    }
}