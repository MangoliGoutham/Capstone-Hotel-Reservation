using HotelReservationSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationSystem.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}
