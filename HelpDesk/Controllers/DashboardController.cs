using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IActivityLogService _activityLogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ITicketService ticketService, IActivityLogService activityLogService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _activityLogService = activityLogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("AdminDashboard"),
                UserRole.Technician => RedirectToAction("TechnicianDashboard"),
                _ => RedirectToAction("UserDashboard")
            };
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var stats = await _ticketService.GetTicketCountByStatusAsync();
            var recentTickets = (await _ticketService.GetAllAsync()).Take(10);
            var recentLogs = await _activityLogService.GetRecentAsync(20);
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");

            ViewBag.Stats = stats;
            ViewBag.RecentTickets = recentTickets;
            ViewBag.RecentLogs = recentLogs;
            ViewBag.Technicians = technicians;
            return View();
        }

        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> TechnicianDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var tickets = await _ticketService.GetByAssigneeIdAsync(user!.Id);
            ViewBag.Tickets = tickets;
            return View();
        }

        public async Task<IActionResult> UserDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var tickets = await _ticketService.GetByUserIdAsync(user!.Id);
            ViewBag.Tickets = tickets;
            return View();
        }
    }
}
