using HotelReservationSystem.DTOs;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;

namespace HotelReservationSystem.Services;

public interface IReservationService
{
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync();
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<ReservationDto?> CreateReservationAsync(int userId, CreateReservationDto createReservationDto);
    Task<ReservationDto?> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto updateDto);
    Task<bool> CancelReservationAsync(int id);
    Task<IEnumerable<ReservationDto>> GetReservationsByUserAsync(int userId);
    Task<IEnumerable<ReservationDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IBillRepository _billRepository;
    private readonly IBillService _billService;
    private readonly INotificationQueue _notificationQueue;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IHotelRepository hotelRepository,
        IBillRepository billRepository,
        IBillService billService,
        INotificationQueue notificationQueue)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _hotelRepository = hotelRepository;
        _billRepository = billRepository;
        _billService = billService;
        _notificationQueue = notificationQueue;
    }

    
    public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        var reservationDtos = new List<ReservationDto>();
        foreach (var reservation in reservations)
        {
            var user = await _userRepository.GetByIdAsync(reservation.UserId);
            var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
            reservationDtos.Add(await MapToDtoAsync(reservation, user, room));
        }
        return reservationDtos;
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;

        var user = await _userRepository.GetByIdAsync(reservation.UserId);
        var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
        return await MapToDtoAsync(reservation, user, room);
    }

    public async Task<ReservationDto?> CreateReservationAsync(int userId, CreateReservationDto createReservationDto)
    {
        var room = await _roomRepository.GetByIdAsync(createReservationDto.RoomId);
        if (room == null) return null;

        if (createReservationDto.CheckInDate >= createReservationDto.CheckOutDate)
        {
            return null; 
        }

        var totalAmount = await CalculateTotalAmountAsync(room.HotelId, room.BasePrice, createReservationDto.CheckInDate, createReservationDto.CheckOutDate);

        var reservation = new Reservation
        {
            ReservationNumber = GenerateReservationNumber(),
            UserId = userId,
            RoomId = createReservationDto.RoomId,
            CheckInDate = createReservationDto.CheckInDate,
            CheckOutDate = createReservationDto.CheckOutDate,
            NumberOfGuests = createReservationDto.NumberOfGuests,
            TotalAmount = totalAmount,
            Status = "Pending",
            SpecialRequests = createReservationDto.SpecialRequests,
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation);

        
        await _notificationQueue.QueueNotificationAsync(new HotelReservationSystem.Models.Notification
        {
            UserId = userId,
            Type = "Booking",
            Message = $"Your reservation #{reservation.ReservationNumber} at {room.Hotel?.Name ?? "our hotel"} is pending confirmation."
        });

        var user = await _userRepository.GetByIdAsync(userId);
        return await MapToDtoAsync(reservation, user, room);
    }
    
    private async Task<decimal> CalculateTotalAmountAsync(int hotelId, decimal basePrice, DateTime checkIn, DateTime checkOut)
    {
        var days = (checkOut - checkIn).Days;
        return await Task.FromResult(basePrice * days);
    }

    public async Task<ReservationDto?> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto updateDto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;

        reservation.Status = updateDto.Status;
        reservation.UpdatedAt = DateTime.UtcNow;

       
        if (updateDto.Status == "Checked-in")
        {
            await UpdateRoomStatus(reservation.RoomId, "Occupied");
            await _notificationQueue.QueueNotificationAsync(new HotelReservationSystem.Models.Notification
            {
                UserId = reservation.UserId,
                Type = "CheckIn",
                Message = $"Welcome! You have successfully checked in to Room {reservation.Room?.RoomNumber}. Enjoy your stay!"
            });
        }
        else if (updateDto.Status == "Checked-out" || updateDto.Status == "CheckedOut")
        {
            await UpdateRoomStatus(reservation.RoomId, "Available");
            await GenerateBillAsync(reservation);
            
            reservation.Status = "Checked-out";

            await _notificationQueue.QueueNotificationAsync(new HotelReservationSystem.Models.Notification
            {
                UserId = reservation.UserId,
                Type = "CheckOut",
                Message = $"Thank you for staying with us! Your check-out is confirmed. Safe travels!"
            });
        }

        await _reservationRepository.UpdateAsync(reservation);

        var user = await _userRepository.GetByIdAsync(reservation.UserId);
        var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
        return await MapToDtoAsync(reservation, user, room);
    }
    
    private async Task GenerateBillAsync(Reservation reservation)
    {
        
        var existingBill = await _billRepository.GetBillByReservationIdAsync(reservation.Id);
        if (existingBill != null) return;

        await _billService.CreateBillForReservationAsync(reservation.Id);
    }

    public async Task<bool> CancelReservationAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null || reservation.Status == "Cancelled") return false;

        reservation.Status = "Cancelled";
        reservation.UpdatedAt = DateTime.UtcNow;
        await _reservationRepository.UpdateAsync(reservation);

        return true;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByUserAsync(int userId)
    {
        var reservations = await _reservationRepository.GetReservationsByUserAsync(userId);
        var reservationDtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var user = await _userRepository.GetByIdAsync(reservation.UserId);
            var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
            reservationDtos.Add(await MapToDtoAsync(reservation, user, room));
        }

        return reservationDtos;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var reservations = await _reservationRepository.GetReservationsByDateRangeAsync(startDate, endDate);
        var reservationDtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var user = await _userRepository.GetByIdAsync(reservation.UserId);
            var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
            reservationDtos.Add(await MapToDtoAsync(reservation, user, room));
        }

        return reservationDtos;
    }

    private async Task UpdateRoomStatus(int roomId, string status)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room != null)
        {
            room.Status = status;
            room.UpdatedAt = DateTime.UtcNow;
            await _roomRepository.UpdateAsync(room);
        }
    }

    private async Task<ReservationDto> MapToDtoAsync(Reservation reservation, User? user, Room? room)
    {
        var hotelName = "";
        if (room != null)
        {
            var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
            hotelName = hotel?.Name ?? "";
        }

        return new ReservationDto
        {
            Id = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            UserId = reservation.UserId,
            RoomId = reservation.RoomId,
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate,
            NumberOfGuests = reservation.NumberOfGuests,
            TotalAmount = reservation.TotalAmount,
            Status = reservation.Status,
            SpecialRequests = reservation.SpecialRequests,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "",
            RoomNumber = room?.RoomNumber ?? "",
            HotelName = hotelName
        };
    }

    private static string GenerateReservationNumber()
    {
        return $"RES{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks % 10000:D4}";
    }
}