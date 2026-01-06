using HotelReservationSystem.DTOs;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;

namespace HotelReservationSystem.Services;

public interface IHotelService
{
    Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
    Task<HotelDto?> GetHotelByIdAsync(int id);
    Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto);
    Task<HotelDto?> UpdateHotelAsync(int id, CreateHotelDto updateHotelDto);
    Task<bool> DeleteHotelAsync(int id);
    Task<IEnumerable<HotelDto>> GetHotelsByCityAsync(string city);
}

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;

    public HotelService(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
    {
        var hotels = await _hotelRepository.GetAllAsync();
        return hotels.Select(MapToDto);
    }

    public async Task<HotelDto?> GetHotelByIdAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        return hotel != null ? MapToDto(hotel) : null;
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto)
    {
        var hotel = new Hotel
        {
            Name = createHotelDto.Name,
            Address = createHotelDto.Address,
            City = createHotelDto.City,
            Country = createHotelDto.Country,
            PhoneNumber = createHotelDto.PhoneNumber,
            Email = createHotelDto.Email,
            StarRating = createHotelDto.StarRating
        };

        await _hotelRepository.AddAsync(hotel);
        return MapToDto(hotel);
    }

    public async Task<HotelDto?> UpdateHotelAsync(int id, CreateHotelDto updateHotelDto)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null) return null;

        hotel.Name = updateHotelDto.Name;
        hotel.Address = updateHotelDto.Address;
        hotel.City = updateHotelDto.City;
        hotel.Country = updateHotelDto.Country;
        hotel.PhoneNumber = updateHotelDto.PhoneNumber;
        hotel.Email = updateHotelDto.Email;
        hotel.StarRating = updateHotelDto.StarRating;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _hotelRepository.UpdateAsync(hotel);
        return MapToDto(hotel);
    }

    public async Task<bool> DeleteHotelAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null) return false;

        await _hotelRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<HotelDto>> GetHotelsByCityAsync(string city)
    {
        var hotels = await _hotelRepository.GetHotelsByCityAsync(city);
        return hotels.Select(MapToDto);
    }

    private static HotelDto MapToDto(Hotel hotel)
    {
        return new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Address = hotel.Address,
            City = hotel.City,
            Country = hotel.Country,
            PhoneNumber = hotel.PhoneNumber,
            Email = hotel.Email,
            StarRating = hotel.StarRating
        };
    }
}