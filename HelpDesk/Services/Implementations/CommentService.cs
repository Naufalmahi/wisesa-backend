using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLog;

        public CommentService(ApplicationDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<Comment> AddCommentAsync(Guid ticketId, Guid userId, string message, bool isInternal)
        {
            var commentType = isInternal ? CommentType.InternalNote : CommentType.Public;

            var comment = new Comment
            {
                TicketId = ticketId,
                UserId = userId,
                Message = message,
                Type = commentType,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);

            // Auto: WaitingUser → InProgress saat user reply (hanya untuk komentar publik)
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null && ticket.Status == TicketStatus.WaitingUser && !isInternal)
            {
                ticket.Status = TicketStatus.InProgress;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _activityLog.LogAsync(ticketId, userId, "Status Changed (Auto)",
                    TicketStatus.WaitingUser.ToString(), TicketStatus.InProgress.ToString());
            }

            await _context.SaveChangesAsync();

            var logAction = isInternal ? "Internal Note Added" : "Comment Added";
            await _activityLog.LogAsync(ticketId, userId, logAction, null,
                message.Length > 100 ? message[..100] + "..." : message);

            return comment;
        }

        // DISINKRONKAN: Menggunakan GetCommentsByTicketIdAsync sesuai kontrak ICommentService
        public async Task<IEnumerable<Comment>> GetCommentsByTicketIdAsync(Guid ticketId, bool includeInternalNotes = false)
        {
            var query = _context.Comments.Include(c => c.User).Where(c => c.TicketId == ticketId);

            if (!includeInternalNotes)
                query = query.Where(c => c.Type == CommentType.Public);

            return await query.OrderBy(c => c.CreatedAt).ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid deletedByUserId)
        {
            var comment = await _context.Comments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return false;
            comment.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Comment?> GetByIdAsync(Guid commentId)
            => await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId);
    }
}