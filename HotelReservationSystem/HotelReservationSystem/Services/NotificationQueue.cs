using HotelReservationSystem.Models;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;

namespace HotelReservationSystem.Services
{
    public interface INotificationQueue
    {
        ValueTask QueueNotificationAsync(Notification notification);
        ValueTask<Notification> DequeueNotificationAsync(CancellationToken cancellationToken);
    }

    public class NotificationQueue : INotificationQueue
    {
        private readonly Channel<Notification> _queue;

        public NotificationQueue()
        {
            
            _queue = Channel.CreateUnbounded<Notification>();
        }

        public async ValueTask QueueNotificationAsync(Notification notification)
        {
            if (notification == null) return;
            await _queue.Writer.WriteAsync(notification);
        }

        public async ValueTask<Notification> DequeueNotificationAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
