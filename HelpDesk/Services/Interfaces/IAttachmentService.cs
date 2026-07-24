using HelpDesk.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<Attachment> UploadAsync(IFormFile file, Guid ticketId, Guid userId);

        Task<IEnumerable<Attachment>> GetByTicketIdAsync(Guid ticketId);

        Task<Attachment?> GetByIdAsync(Guid id);

        Task<bool> DeleteAsync(Guid attachmentId);

        Task<(byte[] data, string contentType, string fileName)?> DownloadAsync(Guid attachmentId);
    }
}