using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;

namespace HelpDesk.Services.Interfaces
{
    public interface IEscalationService
    {
        Task<Escalation> CreateAsync(Guid ticketId, Guid fromUserId, EscalationLevel fromLevel, EscalationLevel toLevel, string reason);
        Task<bool> ApproveAsync(Guid escalationId, Guid toUserId, Guid approvedByUserId);
        Task<bool> RejectAsync(Guid escalationId, Guid rejectedByUserId);
        Task<IEnumerable<Escalation>> GetByTicketIdAsync(Guid ticketId);
        Task<IEnumerable<Escalation>> GetPendingAsync();
    }
}
