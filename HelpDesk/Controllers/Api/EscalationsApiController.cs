using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HelpDesk.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class EscalationsApiController : ControllerBase
    {
        private readonly IEscalationService _escalationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EscalationsApiController(
            IEscalationService escalationService,
            UserManager<ApplicationUser> userManager)
        {
            _escalationService = escalationService;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEscalationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validasi data gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var currentUserId = await GetSafeUserIdAsync();

                var esc = await _escalationService.CreateAsync(
                    dto.TicketId,
                    currentUserId,
                    dto.FromLevel,
                    dto.ToLevel,
                    dto.Reason
                );

                if (esc == null)
                {
                    return BadRequest(new { message = "Gagal membuat eskalasi. Pastikan TicketId valid." });
                }

                return Ok(new
                {
                    esc.Id,
                    Status = esc.Status.ToString(),
                    message = "Pengajuan eskalasi berhasil dibuat."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan server.", detail = ex.Message });
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _escalationService.GetPendingAsync();

            return Ok(list.Select(e => new
            {
                e.Id,
                e.TicketId,
                TicketNumber = e.Ticket?.TicketNumber,
                FromUser = e.FromUser?.Name,
                FromLevel = e.FromLevel.ToString(),
                ToLevel = e.ToLevel.ToString(),
                e.Reason,
                e.CreatedAt
            }));
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveEscalationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = await GetSafeUserIdAsync();
            var ok = await _escalationService.ApproveAsync(id, dto.ToUserId, currentUserId);

            return ok
                ? Ok(new { message = "Eskalasi telah disetujui." })
                : BadRequest(new { message = "Gagal menyetujui eskalasi. Pastikan ID eskalasi dan ToUserId benar." });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var currentUserId = await GetSafeUserIdAsync();
            var ok = await _escalationService.RejectAsync(id, currentUserId);

            return ok
                ? Ok(new { message = "Eskalasi telah ditolak." })
                : BadRequest(new { message = "Gagal menolak eskalasi." });
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetByTicket(Guid ticketId)
        {
            var list = await _escalationService.GetByTicketIdAsync(ticketId);

            return Ok(list.Select(e => new
            {
                e.Id,
                FromUser = e.FromUser?.Name,
                ToUser = e.ToUser?.Name,
                FromLevel = e.FromLevel.ToString(),
                ToLevel = e.ToLevel.ToString(),
                Status = e.Status.ToString(),
                e.Reason,
                e.CreatedAt,
                e.ResolvedAt
            }));
        }
    }

    public class CreateEscalationDto
    {
        [Required(ErrorMessage = "TicketId wajib diisi.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "FromLevel wajib diisi.")]
        public EscalationLevel FromLevel { get; set; }

        [Required(ErrorMessage = "ToLevel wajib diisi.")]
        public EscalationLevel ToLevel { get; set; }

        [Required(ErrorMessage = "Alasan eskalasi (Reason) wajib diisi.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ApproveEscalationDto
    {
        [Required(ErrorMessage = "ToUserId wajib diisi.")]
        public Guid ToUserId { get; set; }
    }
}