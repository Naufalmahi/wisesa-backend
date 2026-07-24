using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var incomingTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);

            var workingTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress);

            var resolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed);

            var totalTeknisi = await _context.Users.CountAsync(u => u.Role == UserRole.Technician);

            return Ok(new
            {
                Stats = new
                {
                    IncomingTickets = incomingTickets,
                    ExitTickets = workingTickets,
                    ResolvedTickets = resolvedTickets,
                    TotalTeknisi = totalTeknisi
                }
            });
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Title,
                    t.Description,
                    t.Priority,
                    Status = t.Status.ToString(),
                    CategoryName = t.Category != null ? t.Category.Name : "General",
                    CreatedBy = t.Creator != null ? t.Creator.Name : "Anonim",
                    AssignedTo = t.Assignee != null ? t.Assignee.Name : "Belum Ditugaskan",
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("users/assign-teknisi-role/{userId}")]
        public async Task<IActionResult> AssignTeknisiRole(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User tidak ditemukan." });
            }

            user.Role = UserRole.Technician;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Berhasil mengubah role '{user.Name}' menjadi Teknisi.",
                UserId = user.Id,
                NewRole = user.Role.ToString()
            });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetTicketReports()
        {
            var totalTickets = await _context.Tickets.CountAsync();
            var openTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            var inProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress);
            var resolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved);
            var closedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed);
            var escalatedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Escalated);

            var totalCompleted = resolvedTickets + closedTickets;
            double completionRate = totalTickets > 0 ? Math.Round((double)totalCompleted / totalTickets * 100, 2) : 0;

            return Ok(new
            {
                Summary = new
                {
                    TotalTickets = totalTickets,
                    TotalCompleted = totalCompleted,
                    CompletionRatePercentage = completionRate
                },
                StatusBreakdown = new
                {
                    Open = openTickets,
                    InProgress = inProgressTickets,
                    Resolved = resolvedTickets,
                    Closed = closedTickets,
                    Escalated = escalatedTickets
                },
                GeneratedAt = DateTime.UtcNow
            });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var adminId = await GetCurrentAdminIdAsync();
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId);

            if (admin == null)
            {
                return NotFound(new { Message = "Profil admin tidak ditemukan." });
            }

            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                Role = admin.Role.ToString(),
                admin.CreatedAt
            });
        }

        private async Task<Guid> GetCurrentAdminIdAsync()
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
            return admin?.Id ?? Guid.Empty;
        }
    }
}