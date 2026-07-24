using HelpDesk.Data;
using HelpDesk.Models.Enums;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Services.Implementations
{
    public class SlaMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SlaMonitorService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public SlaMonitorService(IServiceScopeFactory scopeFactory, ILogger<SlaMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SLA Monitor Service dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var slaService = scope.ServiceProvider.GetRequiredService<ISlaService>();
                    var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Cek & tandai SLA yang breach
                    var breached = await slaService.CheckAndMarkBreachedAsync();
                    foreach (var sla in breached)
                    {
                        // FIX: Menggunakan TechnicianId sebagai target User penerima notif
                        var alreadyNotified = await dbContext.Notifications.AnyAsync(n =>
                            n.TicketId == sla.TicketId
                            && n.UserId == sla.TechnicianId
                            && n.Type == NotificationType.SLABreached, stoppingToken);

                        if (!alreadyNotified)
                        {
                            _logger.LogWarning("SLA BREACH: Ticket {TicketId} terdeteksi terlambat!", sla.TicketId);

                            await notifService.CreateAsync(new Models.Entities.Notification
                            {
                                UserId = sla.TechnicianId,
                                TicketId = sla.TicketId,
                                Type = NotificationType.SLABreached,
                                Message = "SLA telah dilanggar! Segera selesaikan ticket ini."
                            });
                        }
                    }

                    // 2. Cek SLA mendekati breach (75%) — kirim warning SEKALI saja
                    var nearBreach = await slaService.GetNearBreachAsync(75);
                    foreach (var sla in nearBreach)
                    {
                        // FIX: Menggunakan TechnicianId
                        var alreadyWarned = await dbContext.Notifications.AnyAsync(n =>
                            n.TicketId == sla.TicketId
                            && n.UserId == sla.TechnicianId
                            && n.Type == NotificationType.SLAWarning, stoppingToken);

                        if (!alreadyWarned)
                        {
                            _logger.LogInformation("SLA WARNING: Ticket {TicketId} mendekati 75% deadline.", sla.TicketId);

                            await notifService.CreateAsync(new Models.Entities.Notification
                            {
                                UserId = sla.TechnicianId,
                                TicketId = sla.TicketId,
                                Type = NotificationType.SLAWarning,
                                Message = "Peringatan: SLA ticket ini mendekati batas waktu (75%)."
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi error pada proses background SLA Monitor.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("SLA Monitor Service dihentikan.");
        }
    }
}