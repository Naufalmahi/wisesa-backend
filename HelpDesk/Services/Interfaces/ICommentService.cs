using HelpDesk.Models.Entities;

namespace HelpDesk.Services.Interfaces
{
    public interface ICommentService
    {
        Task<Comment> AddCommentAsync(Guid ticketId, Guid userId, string message, bool isInternal);

        Task<IEnumerable<Comment>> GetCommentsByTicketIdAsync(Guid ticketId, bool includeInternalNotes = false);

        Task<bool> DeleteCommentAsync(Guid commentId, Guid deletedByUserId);
        Task<Comment?> GetByIdAsync(Guid commentId);
    }
}