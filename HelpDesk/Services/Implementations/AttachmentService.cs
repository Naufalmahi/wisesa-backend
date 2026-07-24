using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AttachmentService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Attachment> UploadAsync(IFormFile file, Guid ticketId, Guid userId)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", ticketId.ToString());
            Directory.CreateDirectory(folder);

            var storedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, storedName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UploadedByUserId = userId,
                FileName = file.FileName,
                StoredFileName = storedName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<IEnumerable<Attachment>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.TicketId == ticketId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(Guid id)
            => await _context.Attachments.FindAsync(id);

        public async Task<bool> DeleteAsync(Guid attachmentId)
        {
            var att = await _context.Attachments.FindAsync(attachmentId);
            if (att == null) return false;

            var filePath = Path.Combine(_env.WebRootPath, "uploads", att.TicketId.ToString(), att.StoredFileName);
            if (File.Exists(filePath)) File.Delete(filePath);

            _context.Attachments.Remove(att);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] data, string contentType, string fileName)?> DownloadAsync(Guid attachmentId)
        {
            var att = await _context.Attachments.FindAsync(attachmentId);
            if (att == null) return null;

            var filePath = Path.Combine(_env.WebRootPath, "uploads", att.TicketId.ToString(), att.StoredFileName);
            if (!File.Exists(filePath)) return null;

            var data = await File.ReadAllBytesAsync(filePath);
            return (data, att.ContentType, att.FileName);
        }
    }
}