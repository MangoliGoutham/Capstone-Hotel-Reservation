using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelReservationSystem.DTOs;
using HotelReservationSystem.Services;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllHotels()
    {
        var hotels = await _hotelService.GetAllHotelsAsync();
        return Ok(hotels);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHotelById(int id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return Ok(hotel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDto createHotelDto)
    {
        var hotel = await _hotelService.CreateHotelAsync(createHotelDto);
        return CreatedAtAction(nameof(GetHotelById), new { id = hotel.Id }, hotel);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HotelManager")]
    public async Task<IActionResult> UpdateHotel(int id, [FromBody] CreateHotelDto updateHotelDto)
    {
        var hotel = await _hotelService.UpdateHotelAsync(id, updateHotelDto);
        if (hotel == null)
        {
            return NotFound();
        }
        return Ok(hotel);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await _hotelService.DeleteHotelAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("city/{city}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHotelsByCity(string city)
    {
        var hotels = await _hotelService.GetHotelsByCityAsync(city);
        return Ok(hotels);
    }
}