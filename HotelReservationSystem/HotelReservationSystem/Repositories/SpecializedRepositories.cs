using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HotelDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetStaffUsersAsync()
    {
        return await _dbSet
            .Where(u => u.Role == "Receptionist" || u.Role == "HotelManager")
            .OrderBy(u => u.LastName)
            .ToListAsync();
    }
}

public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
    {
        return await _dbSet
            .Where(h => h.City.ToLower().Contains(city.ToLower()))
            .ToListAsync();
    }
}

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? hotelId = null)
    {
        var query = _dbSet
            .Include(r => r.Hotel)
            .Where(r => r.Status == "Available")
            .Where(r => !r.Reservations.Any(res => 
                res.Status != "Cancelled" && 
                ((checkIn >= res.CheckInDate && checkIn < res.CheckOutDate) ||
                 (checkOut > res.CheckInDate && checkOut <= res.CheckOutDate) ||
                 (checkIn <= res.CheckInDate && checkOut >= res.CheckOutDate))));

        if (hotelId.HasValue)
        {
            query = query.Where(r => r.HotelId == hotelId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetRoomsByHotelAsync(int hotelId)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Where(r => r.HotelId == hotelId)
            .ToListAsync();
    }
}

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
    {
        return await _dbSet
            .Include(r => r.Room)
            .ThenInclude(rm => rm.Hotel)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(r => r.Room)
            .ThenInclude(rm => rm.Hotel)
            .Include(r => r.User)
            .Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate)
            .OrderBy(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        var query = _dbSet
            .Where(r => r.RoomId == roomId && r.Status != "Cancelled")
            .Where(r => (checkIn >= r.CheckInDate && checkIn < r.CheckOutDate) ||
                       (checkOut > r.CheckInDate && checkOut <= r.CheckOutDate) ||
                       (checkIn <= r.CheckInDate && checkOut >= r.CheckOutDate));

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return !await query.AnyAsync();
    }
}

public class BillRepository : Repository<Bill>, IBillRepository
{
    public BillRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Bill>> GetBillsByReservationAsync(int reservationId)
    {
        return await _dbSet
            .Include(b => b.Reservation)
            .Where(b => b.ReservationId == reservationId)
            .ToListAsync();
    }

    public async Task<Bill?> GetBillByReservationIdAsync(int reservationId)
    {
        return await _dbSet
            .Include(b => b.Reservation)
            .FirstOrDefaultAsync(b => b.ReservationId == reservationId);
    }
}