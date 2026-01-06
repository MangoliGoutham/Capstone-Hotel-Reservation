using HotelReservationSystem.DTOs;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;

namespace HotelReservationSystem.Services;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
    Task<RoomDto?> GetRoomByIdAsync(int id);
    Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto);
    Task<RoomDto?> UpdateRoomAsync(int id, CreateRoomDto updateRoomDto);
    Task<bool> DeleteRoomAsync(int id);
    Task<IEnumerable<RoomDto>> GetRoomsByHotelAsync(int hotelId);
    Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(RoomSearchDto searchDto);
    Task<bool> UpdateRoomStatusAsync(int id, string status);
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IHotelRepository _hotelRepository;

    public RoomService(IRoomRepository roomRepository, IHotelRepository hotelRepository)
    {
        _roomRepository = roomRepository;
        _hotelRepository = hotelRepository;
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        var roomDtos = new List<RoomDto>();

        foreach (var room in rooms)
        {
            var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
            roomDtos.Add(MapToDto(room, hotel?.Name ?? ""));
        }

        return roomDtos;
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return null;

        var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
        return MapToDto(room, hotel?.Name ?? "");
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto)
    {
        var room = new Room
        {
            RoomNumber = createRoomDto.RoomNumber,
            RoomType = createRoomDto.RoomType,
            BasePrice = createRoomDto.BasePrice,
            Capacity = createRoomDto.Capacity,
            Description = createRoomDto.Description,
            HotelId = createRoomDto.HotelId,
            Status = "Available"
        };

        await _roomRepository.AddAsync(room);
        var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
        return MapToDto(room, hotel?.Name ?? "");
    }

    public async Task<RoomDto?> UpdateRoomAsync(int id, CreateRoomDto updateRoomDto)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return null;

        room.RoomNumber = updateRoomDto.RoomNumber;
        room.RoomType = updateRoomDto.RoomType;
        room.BasePrice = updateRoomDto.BasePrice;
        room.Capacity = updateRoomDto.Capacity;
        room.Description = updateRoomDto.Description;
        room.HotelId = updateRoomDto.HotelId;
        room.UpdatedAt = DateTime.UtcNow;

        await _roomRepository.UpdateAsync(room);
        var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
        return MapToDto(room, hotel?.Name ?? "");
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return false;

        await _roomRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsByHotelAsync(int hotelId)
    {
        var rooms = await _roomRepository.GetRoomsByHotelAsync(hotelId);
        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        return rooms.Select(r => MapToDto(r, hotel?.Name ?? ""));
    }

    public async Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(RoomSearchDto searchDto)
    {
        var rooms = await _roomRepository.GetAvailableRoomsAsync(
            searchDto.CheckInDate, 
            searchDto.CheckOutDate, 
            searchDto.HotelId);

        var filteredRooms = rooms.Where(r => r.Capacity >= searchDto.NumberOfGuests);

        if (!string.IsNullOrEmpty(searchDto.RoomType))
        {
            filteredRooms = filteredRooms.Where(r => 
                r.RoomType.Equals(searchDto.RoomType, StringComparison.OrdinalIgnoreCase));
        }

        var roomDtos = new List<RoomDto>();
        foreach (var room in filteredRooms)
        {
            var hotel = await _hotelRepository.GetByIdAsync(room.HotelId);
            roomDtos.Add(MapToDto(room, hotel?.Name ?? ""));
        }

        return roomDtos;
    }

    public async Task<bool> UpdateRoomStatusAsync(int id, string status)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return false;

        room.Status = status;
        room.UpdatedAt = DateTime.UtcNow;
        await _roomRepository.UpdateAsync(room);
        return true;
    }

    private static RoomDto MapToDto(Room room, string hotelName)
    {
        return new RoomDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            BasePrice = room.BasePrice,
            Capacity = room.Capacity,
            Description = room.Description,
            Status = room.Status,
            HotelId = room.HotelId,
            HotelName = hotelName
        };
    }
}