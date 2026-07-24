using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpDesk.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsApiController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        private async Task<Guid> GetSafeUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && Guid.TryParse(claim.Value, out var parsedGuid))
                {
                    return parsedGuid;
                }
            }

            var adminUser = await _userManager.FindByEmailAsync("admin@helpdesk.com");
            if (adminUser != null)
            {
                return adminUser.Id;
            }

            var firstUser = _userManager.Users.FirstOrDefault();
            return firstUser?.Id ?? Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false)
        {
            var userId = await GetSafeUserIdAsync();

            if (userId == Guid.Empty)
            {
                return Ok(new List<object>()); // Kembalikan list kosong jika tidak ada user di DB
            }

            var list = await _notificationService.GetByUserIdAsync(userId, unreadOnly);

            return Ok(list.Select(n => new
            {
                n.Id,
                n.Message,
                Type = n.Type.ToString(),
                n.IsRead,
                n.TicketId,
                TicketNumber = n.Ticket?.TicketNumber,
                n.CreatedAt
            }));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = await GetSafeUserIdAsync();

            if (userId == Guid.Empty)
            {
                return Ok(new { count = 0 });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Notifikasi ditandai dibaca." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = await GetSafeUserIdAsync();

            if (userId != Guid.Empty)
            {
                await _notificationService.MarkAllAsReadAsync(userId);
            }

            return Ok(new { message = "Semua notifikasi ditandai dibaca." });
        }
    }
}