using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelReservationSystem.DTOs;
using HotelReservationSystem.Services;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HotelManager")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createRoomDto)
    {
        var room = await _roomService.CreateRoomAsync(createRoomDto);
        return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HotelManager")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] CreateRoomDto updateRoomDto)
    {
        var room = await _roomService.UpdateRoomAsync(id, updateRoomDto);
        if (room == null)
        {
            return NotFound();
        }
        return Ok(room);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var result = await _roomService.DeleteRoomAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("hotel/{hotelId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomsByHotel(int hotelId)
    {
        var rooms = await _roomService.GetRoomsByHotelAsync(hotelId);
        return Ok(rooms);
    }

    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchAvailableRooms([FromBody] RoomSearchDto searchDto)
    {
        var rooms = await _roomService.SearchAvailableRoomsAsync(searchDto);
        return Ok(rooms);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] UpdateRoomStatusDto statusDto)
    {
        var result = await _roomService.UpdateRoomStatusAsync(id, statusDto.Status);
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { message = "Room status updated successfully" });
    }
}