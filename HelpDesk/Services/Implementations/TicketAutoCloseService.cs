using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using HelpDesk.Models.Entities;

namespace HelpDesk.Services.Implementations
{
    public class TicketAutoCloseService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TicketAutoCloseService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public TicketAutoCloseService(IServiceScopeFactory scopeFactory, ILogger<TicketAutoCloseService> logger)
        { _scopeFactory = scopeFactory; _logger = logger; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ticket Auto-Close Service dimulai.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var adminUser = await userManager.FindByEmailAsync("admin@helpdesk.com");
                    var systemUserId = adminUser?.Id ?? Guid.Empty;

                    var tickets = await ticketService.GetAutoCloseableTicketsAsync();
                    foreach (var ticket in tickets)
                    {
                        await ticketService.CloseTicketAsync(ticket.Id, systemUserId);

                        await notifService.CreateAsync(new Models.Entities.Notification
                        {
                            UserId = ticket.UserId, TicketId = ticket.Id,
                            Type = NotificationType.TicketClosed,
                            Message = $"Ticket {ticket.TicketNumber} ditutup otomatis setelah 24 jam."
                        });

                        _logger.LogInformation("Auto-closed: {TicketNumber}", ticket.TicketNumber);
                    }
                }
                catch (Exception ex) { _logger.LogError(ex, "Error pada Auto-Close Service."); }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
