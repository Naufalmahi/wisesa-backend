using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using HelpDesk.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HelpDesk.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsApiController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IAttachmentService _attachmentService;
        private readonly ICommentService _commentService;
        private readonly ISlaService _slaService;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsApiController(ITicketService ticketService, IAttachmentService attachmentService,
            ICommentService commentService, ISlaService slaService, IActivityLogService activityLogService,
            INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _attachmentService = attachmentService;
            _commentService = commentService;
            _slaService = slaService;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validasi gagal. Mohon periksa kembali format data yang Anda kirim.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var adminUser = await _userManager.FindByEmailAsync("admin@helpdesk.com");
                var fallbackUserId = adminUser?.Id ?? Guid.Empty;
                var finalUserId = User.Identity?.IsAuthenticated == true ? GetUserId() : fallbackUserId;

                var ticket = new Ticket
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    CategoryId = dto.CategoryId,
                    AffectedUser = dto.AffectedUser,
                    RelatedTicketId = dto.RelatedTicketId,
                    UserId = finalUserId
                };

                var created = await _ticketService.CreateTicketAsync(ticket);

                if (created == null)
                {
                    return BadRequest(new
                    {
                        message = "Gagal membuat ticket. Pastikan CategoryId yang Anda masukkan benar-benar ada di database."
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
                {
                    created.Id,
                    created.TicketNumber,
                    message = "Ticket berhasil dibuat tanpa token keamanan."
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan internal pada server saat menyimpan tiket.",
                    detail = innerMessage
                });
            }
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetAll([FromQuery] TicketStatus? status,
            [FromQuery] TicketPriority? priority, [FromQuery] Guid? categoryId)
        {
            IEnumerable<Ticket> tickets;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.FindByIdAsync(GetUserId().ToString());
                if (user == null) return Unauthorized();

                tickets = user.Role switch
                {
                    UserRole.Admin => await _ticketService.GetAllAsync(status, priority, categoryId),
                    UserRole.Technician => await _ticketService.GetByAssigneeIdAsync(user.Id),
                    _ => await _ticketService.GetByUserIdAsync(user.Id)
                };

                if (status.HasValue && user.Role != UserRole.Admin)
                    tickets = tickets.Where(t => t.Status == status.Value);
            }
            else
            {
                tickets = await _ticketService.GetAllAsync(status, priority, categoryId);
            }

            return Ok(tickets.Select(t => new
            {
                t.Id,
                t.TicketNumber,
                t.Title,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Category = t.Category?.Name,
                Creator = t.Creator?.Name,
                Assignee = t.Assignee?.Name,
                t.CreatedAt,
                t.UpdatedAt
            }));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            var includeInternal = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.FindByIdAsync(GetUserId().ToString());
                if (user != null)
                {
                    if (user.Role == UserRole.User && ticket.UserId != user.Id) return Forbid();
                    includeInternal = user.Role != UserRole.User;
                }
            }

            var sla = await _slaService.GetActiveSlaByTicketIdAsync(id);
            var logs = await _activityLogService.GetLogsByTicketIdAsync(id);
            var attachments = await _attachmentService.GetByTicketIdAsync(id);
            var comments = await _commentService.GetCommentsByTicketIdAsync(id, includeInternal);

            var response = new TicketDetailResponse
            {
                Ticket = new TicketInfoResponse
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    AffectedUser = ticket.AffectedUser,
                    RelatedTicketId = ticket.RelatedTicketId,
                    Status = ticket.Status.ToString(),
                    Priority = ticket.Priority.ToString(),
                    Category = ticket.Category?.Name,
                    Creator = ticket.Creator?.Name,
                    Assignee = ticket.Assignee?.Name,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    ClosedAt = ticket.ClosedAt
                },
                Comments = comments.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    Message = c.Message,
                    Type = c.Type.ToString(),
                    User = c.User?.Name,
                    CreatedAt = c.CreatedAt
                }),
                Sla = sla != null ? new SlaResponse
                {
                    DeadlineAt = sla.DeadlineAt,
                    IsBreached = sla.IsBreached,
                    BreachedAt = sla.BreachedAt
                } : null,
                Attachments = attachments.Select(a => new AttachmentResponse
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    CreatedAt = a.CreatedAt
                }),
                ActivityLogs = logs.Select(l => new ActivityLogResponse
                {
                    Action = l.Action,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    User = l.User?.Name,
                    CreatedAt = l.CreatedAt
                })
            };

            return Ok(response);
        }

        [HttpPut("{id}/status")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician,Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _ticketService.UpdateStatusAsync(id, dto.Status, GetUserId());
            return ok ? Ok(new { message = "Status berhasil diubah." }) : NotFound();
        }

        [HttpPut("{id}/assign")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Assign(Guid id, [FromBody] AssignDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _ticketService.AssignTicketAsync(id, dto.TechnicianId, GetUserId());
            if (!ok) return NotFound();

            await _slaService.CreateTicketSlaAsync(id, dto.TechnicianId);
            return Ok(new { message = "Ticket berhasil di-assign." });
        }

        [HttpPost("{id}/reopen")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Reopen(Guid id)
        {
            var ok = await _ticketService.ReopenTicketAsync(id, GetUserId());
            return ok ? Ok(new { message = "Ticket dibuka kembali." })
                      : BadRequest(new { message = "Gagal membuka kembali ticket." });
        }

        [HttpPost("{id}/comments")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(GetUserId().ToString());
            if (user == null) return Unauthorized();

            var type = user.Role == UserRole.User ? CommentType.Public : dto.Type;
            bool isInternal = (type == CommentType.InternalNote);

            var comment = await _commentService.AddCommentAsync(id, GetUserId(), dto.Message, isInternal);

            return Ok(new
            {
                comment.Id,
                comment.Message,
                Type = comment.Type.ToString(),
                comment.CreatedAt
            });
        }

        [HttpPost("{id}/attachments")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(Guid id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "File tidak valid." });
            if (file.Length > 10 * 1024 * 1024) return BadRequest(new { message = "Max ukuran file 10MB." });

            var att = await _attachmentService.UploadAsync(file, id, GetUserId());
            return Ok(new { att.Id, att.FileName, att.FileSize, att.ContentType });
        }

        [HttpGet("attachments/{attachmentId}/download")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
        {
            var fileResult = await _attachmentService.DownloadAsync(attachmentId);
            if (fileResult == null)
            {
                return NotFound(new { message = "File atau data fisik tidak ditemukan di server." });
            }
            return File(fileResult.Value.data, fileResult.Value.contentType, fileResult.Value.fileName);
        }

        [HttpGet("stats")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Stats()
        {
            var stats = await _ticketService.GetTicketCountByStatusAsync();
            return Ok(stats.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
        }

        [HttpGet("my-stats")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> MyStats()
        {
            var userId = GetUserId();
            var tickets = await _ticketService.GetByUserIdAsync(userId);

            var stats = tickets
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return Ok(stats);
        }
    }

    public class CreateTicketDto
    {
        [Required(ErrorMessage = "Title wajib diisi.")]
        [StringLength(150, ErrorMessage = "Title tidak boleh melebihi 150 karakter.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description wajib diisi.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority wajib ditentukan.")]
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        [Required(ErrorMessage = "Category wajib dipilih (CategoryId tidak boleh kosong).")]
        public Guid CategoryId { get; set; }

        public string? AffectedUser { get; set; }

        public Guid? RelatedTicketId { get; set; }
    }

    public class UpdateStatusDto
    {
        [Required]
        public TicketStatus Status { get; set; }
    }

    public class AssignDto
    {
        [Required]
        public Guid TechnicianId { get; set; }
    }

    public class AddCommentDto
    {
        [Required(ErrorMessage = "Pesan komentar tidak boleh kosong.")]
        public string Message { get; set; } = string.Empty;
        public CommentType Type { get; set; } = CommentType.Public;
    }
}