using HotelReservationSystem.Models;

namespace HotelReservationSystem.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetStaffUsersAsync();
}

public interface IHotelRepository : IRepository<Hotel>
{
    Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
}

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? hotelId = null);
    Task<IEnumerable<Room>> GetRoomsByHotelAsync(int hotelId);
}

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
    Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}

public interface IBillRepository : IRepository<Bill>
{
    Task<IEnumerable<Bill>> GetBillsByReservationAsync(int reservationId);
    Task<Bill?> GetBillByReservationIdAsync(int reservationId);
}