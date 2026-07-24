using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeknisiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeknisiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var teknisiId = await GetCurrentUserIdAsync();
            if (teknisiId == Guid.Empty)
            {
                return BadRequest(new { Message = "Tidak ada User/Teknisi yang terdaftar di database." });
            }

            var today = DateTime.UtcNow.Date;

            var incomingTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            var workingTickets = await _context.Tickets.CountAsync(t => t.AssignedToId == teknisiId && t.Status == TicketStatus.InProgress);
            var resolvedToday = await _context.Tickets.CountAsync(t => t.AssignedToId == teknisiId
                                 && (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
                                 && t.UpdatedAt >= today);

            var totalAllTickets = await _context.Tickets.CountAsync();
            var totalOpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            var totalInProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress);
            var totalResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed);

            var complaints = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Creator)
                .Where(t => t.AssignedToId == teknisiId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Title,
                    t.Description,
                    t.Priority,
                    Status = t.Status.ToString(),
                    CategoryName = t.Category != null ? t.Category.Name : "General",
                    UserName = t.Creator != null ? t.Creator.Name : "Anonim",
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                Stats = new
                {
                    IncomingTicket = incomingTickets,
                    ExitTicket = workingTickets,
                    ResolvedToday = resolvedToday
                },
                GlobalSummary = new
                {
                    TotalTickets = totalAllTickets,
                    TotalOpen = totalOpenTickets,
                    TotalInProgress = totalInProgressTickets,
                    TotalResolved = totalResolvedTickets
                },
                RecentComplaints = complaints
            });
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetTicketQueue()
        {
            var queues = await _context.Tickets
                .Where(t => t.Status == TicketStatus.Open)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Title,
                    t.Description,
                    t.Priority,
                    Status = t.Status.ToString(),
                    t.CreatedAt,
                    t.UserId,
                    t.CategoryId,
                    CategoryName = t.Category != null ? t.Category.Name : "General"
                })
                .ToListAsync();

            return Ok(queues);
        }

        [HttpPost("queue/pick/{ticketId}")]
        public async Task<IActionResult> PickTicket(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound(new { Message = "Tiket tidak ditemukan." });

            if (ticket.Status != TicketStatus.Open)
            {
                return BadRequest(new { Message = "Tiket ini sudah diambil atau tidak dalam status Open." });
            }

            var teknisiId = await GetCurrentUserIdAsync();
            if (teknisiId == Guid.Empty)
                return BadRequest(new { Message = "Tidak ada User/Teknisi yang valid di database." });

            var now = DateTime.UtcNow;

            ticket.AssignedToId = teknisiId;
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = now;

            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = teknisiId,
                Action = "Teknisi mengambil tiket dari antrean dan memulai pengerjaan (Batas waktu: 4 Jam).",
                CreatedAt = now
            });

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = teknisiId,
                Type = NotificationType.TicketAssigned,
                Message = $"Anda telah mengambil tiket #{ticket.TicketNumber}. Batas waktu pengerjaan: 4 Jam dari sekarang.",
                IsRead = false,
                CreatedAt = now
            });

            if (ticket.UserId != Guid.Empty)
            {
                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    UserId = ticket.UserId,
                    Type = NotificationType.TicketUpdated,
                    Message = $"Tiket #{ticket.TicketNumber} Anda telah diambil oleh teknisi dan sedang diproses.",
                    IsRead = false,
                    CreatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            var deadline = now.AddHours(4);

            return Ok(new
            {
                Message = "Tiket berhasil diambil. Pengerjaan telah dimulai.",
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                StartTime = now,
                Deadline = deadline,
                RemainingSeconds = 4 * 3600 
            });
        }

        [HttpGet("active-ticket")]
        public async Task<IActionResult> GetActiveTicket()
        {
            var teknisiId = await GetCurrentUserIdAsync();

            var activeTicket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.AssignedToId == teknisiId && t.Status == TicketStatus.InProgress);

            if (activeTicket == null)
                return NotFound(new { Message = "Tidak ada tiket yang sedang dikerjakan saat ini." });

            var now = DateTime.UtcNow;
            var deadline = activeTicket.UpdatedAt.AddHours(4);
            var remainingTimeSpan = deadline - now;
            var remainingSeconds = Math.Max(0, (int)remainingTimeSpan.TotalSeconds);

            return Ok(new
            {
                Ticket = activeTicket,
                StartTime = activeTicket.UpdatedAt,
                Deadline = deadline,
                RemainingSeconds = remainingSeconds,
                IsExpired = remainingSeconds == 0
            });
        }

        [HttpPost("tickets/{ticketId}/complete")]
        public async Task<IActionResult> CompleteTicket(Guid ticketId, [FromBody] CompleteTicketDto request)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound(new { Message = "Tiket tidak ditemukan." });

            var teknisiId = await GetCurrentUserIdAsync();
            if (ticket.AssignedToId != teknisiId)
            {
                return BadRequest(new { Message = "Anda tidak memiliki wewenang untuk menyelesaikan tiket ini." });
            }

            var now = DateTime.UtcNow;

            ticket.Status = TicketStatus.Resolved;
            ticket.UpdatedAt = now;
            ticket.ClosedAt = now;

            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = teknisiId,
                Action = $"Pengerjaan tiket selesai dikerjakan. Catatan Penyelesaian: {request.ResolutionNote}",
                CreatedAt = now
            });

            if (ticket.UserId != Guid.Empty)
            {
                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    UserId = ticket.UserId,
                    Type = NotificationType.TicketUpdated,
                    Message = $"Tiket #{ticket.TicketNumber} telah selesai dikerjakan oleh teknisi.",
                    IsRead = false,
                    CreatedAt = now
                });
            }

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = teknisiId,
                Type = NotificationType.TicketUpdated,
                Message = $"Selamat! Anda telah menyelesaikan tiket #{ticket.TicketNumber}.",
                IsRead = false,
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Pengerjaan tiket dihentikan dan tiket dinyatakan Selesai (Resolved).", Ticket = ticket });
        }

        [HttpGet("tickets/{ticketId}/notes")]
        public async Task<IActionResult> GetInternalNotes(Guid ticketId)
        {
            var notes = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.TicketId,
                    c.UserId,
                    AuthorName = c.User != null ? c.User.Name : "Teknisi",
                    c.Message,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(notes);
        }

        [HttpPost("tickets/{ticketId}/notes")]
        public async Task<IActionResult> AddInternalNote(Guid ticketId, [FromBody] NoteRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { Message = "Catatan internal tidak boleh kosong!" });

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound(new { Message = "Tiket tidak ditemukan." });

            var teknisiId = await GetCurrentUserIdAsync();
            var now = DateTime.UtcNow;

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = teknisiId,
                Message = request.Message,
                CreatedAt = now
            };

            _context.Comments.Add(comment);

            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = teknisiId,
                Action = $"Menambahkan catatan internal: \"{request.Message}\"",
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Catatan internal berhasil ditambahkan.", Note = comment });
        }

        [HttpPost("tickets/{ticketId}/escalate")]
        public async Task<IActionResult> SubmitEscalation(Guid ticketId, [FromBody] NoteRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { Message = "Alasan eskalasi tidak boleh kosong." });
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new { Message = "Tiket tidak ditemukan." });
            }

            var now = DateTime.UtcNow;
            ticket.Status = TicketStatus.Escalated;
            ticket.UpdatedAt = now;

            var userId = await GetCurrentUserIdAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = userId,
                Action = $"Tiket dieskalasi. Alasan: {request.Message}",
                CreatedAt = now
            });

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = ticket.UserId,
                Type = NotificationType.TicketAssigned,
                Message = $"Tiket #{ticket.TicketNumber} Anda telah dieskalasi dengan alasan: {request.Message}",
                IsRead = false,
                CreatedAt = now
            });

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = userId,
                Type = NotificationType.TicketAssigned,
                Message = $"Anda berhasil melakukan eskalasi pada tiket #{ticket.TicketNumber}.",
                IsRead = false,
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Tiket berhasil dieskalasi.",
                Ticket = ticket
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetCompletedHistory()
        {
            var teknisiId = await GetCurrentUserIdAsync();

            var history = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Creator)
                .Where(t => t.AssignedToId == teknisiId &&
                           (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed))
                .OrderByDescending(t => t.ClosedAt)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Title,
                    t.Description,
                    t.Priority,
                    Status = t.Status.ToString(),
                    CategoryName = t.Category != null ? t.Category.Name : "General",
                    CustomerName = t.Creator != null ? t.Creator.Name : "Anonim",
                    t.CreatedAt,
                    t.ClosedAt,
                    Duration = t.ClosedAt.HasValue ? (t.ClosedAt.Value - t.UpdatedAt).TotalMinutes + " Menit" : "-"
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = await GetCurrentUserIdAsync();
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync();
            return user?.Id ?? Guid.Empty;
        }

        public class NoteRequestDto
        {
            public string Message { get; set; } = string.Empty;
        }

        public class CompleteTicketDto
        {
            public string ResolutionNote { get; set; } = string.Empty;
        }
    }
}