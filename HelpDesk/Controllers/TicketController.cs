using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ICategoryService _categoryService;
        private readonly ICommentService _commentService;
        private readonly ISlaService _slaService;
        private readonly INotificationService _notificationService;
        private readonly IAttachmentService _attachmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(ITicketService ticketService, ICategoryService categoryService,
            ICommentService commentService, ISlaService slaService, INotificationService notificationService,
            IAttachmentService attachmentService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _categoryService = categoryService;
            _commentService = commentService;
            _slaService = slaService;
            _notificationService = notificationService;
            _attachmentService = attachmentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(TicketStatus? status, TicketPriority? priority, Guid? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IEnumerable<Ticket> tickets = user.Role switch
            {
                UserRole.Admin => await _ticketService.GetAllAsync(status, priority, categoryId),
                UserRole.Technician => await _ticketService.GetByAssigneeIdAsync(user.Id),
                _ => await _ticketService.GetByUserIdAsync(user.Id)
            };

            if (status.HasValue && user.Role != UserRole.Admin)
                tickets = tickets.Where(t => t.Status == status.Value);

            return View(tickets);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();


            if (user.Role == UserRole.User && ticket.UserId != user.Id) return Forbid();

            ViewBag.ActiveSla = await _slaService.GetActiveSlaByTicketIdAsync(id);
            ViewBag.Attachments = await _attachmentService.GetByTicketIdAsync(id);


            bool includeInternal = user.Role != UserRole.User;
            ViewBag.Comments = await _commentService.GetCommentsByTicketIdAsync(id, includeInternal);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid ticketId, string message, CommentType type)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Pesan komentar tidak boleh kosong.";
                return RedirectToAction("Detail", new { id = ticketId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.Role == UserRole.User) type = CommentType.Public;

            bool isInternal = (type == CommentType.InternalNote);
            await _commentService.AddCommentAsync(ticketId, user.Id, message, isInternal);

            TempData["Success"] = "Komentar berhasil ditambahkan.";
            return RedirectToAction("Detail", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachment(Guid ticketId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "File tidak valid.";
                return RedirectToAction("Detail", new { id = ticketId });
            }
            if (file.Length > 10 * 1024 * 1024) 
            {
                TempData["Error"] = "Ukuran file maksimal adalah 10MB.";
                return RedirectToAction("Detail", new { id = ticketId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            try
            {
                await _attachmentService.UploadAsync(file, ticketId, user.Id);
                TempData["Success"] = "File berhasil diunggah.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Gagal mengunggah file: {ex.Message}";
            }

            return RedirectToAction("Detail", new { id = ticketId });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
        {
            var fileResult = await _attachmentService.DownloadAsync(attachmentId);

            if (fileResult == null)
            {
                TempData["Error"] = "File fisik tidak ditemukan di server.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/Ticket");
            }

            return File(fileResult.Value.data, fileResult.Value.contentType, fileResult.Value.fileName);
        }
    }
}