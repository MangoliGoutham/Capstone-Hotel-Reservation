using HotelReservationSystem.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservationSystem.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly INotificationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            INotificationQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notification = await _queue.DequeueNotificationAsync(stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                        await repository.AddAsync(notification);
                        
                        
                        _logger.LogInformation($"Notification processed for User {notification.UserId}: {notification.Message}");
                        
                        await SimulateExternalNotificationAsync(notification);
                    }
                }
                catch (OperationCanceledException)
                {
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification.");
                }
            }
        }

        private Task SimulateExternalNotificationAsync(HotelReservationSystem.Models.Notification notification)
        {
            // Simulate delay for external API call (e.g., SendGrid, Twilio)
            return Task.Delay(100); 
        }
    }
}
