using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HotelReservationSystem.DTOs;
using HotelReservationSystem.Services;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetAllReservations()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationById(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        // check if user can access this reservation
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        
        if (userRole == "Guest" && reservation.UserId != userId)
        {
            return Forbid();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createReservationDto)
    {
        var userId = GetCurrentUserId();
        var reservation = await _reservationService.CreateReservationAsync(userId, createReservationDto);
        
        if (reservation == null)
        {
            return BadRequest(new { message = "Room is not available for the selected dates or doesn't meet capacity requirements" });
        }

        return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, reservation);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] UpdateReservationStatusDto updateDto)
    {
        var reservation = await _reservationService.UpdateReservationStatusAsync(id, updateDto);
        if (reservation == null)
        {
            return NotFound();
        }
        return Ok(reservation);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        // check if user can cancel this reservation
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        
        if (userRole == "Guest" && reservation.UserId != userId)
        {
            return Forbid();
        }

        var result = await _reservationService.CancelReservationAsync(id);
        if (!result)
        {
            return BadRequest(new { message = "Unable to cancel reservation" });
        }

        return Ok(new { message = "Reservation cancelled successfully" });
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetReservationsByUser(int userId)
    {
        var reservations = await _reservationService.GetReservationsByUserAsync(userId);
        return Ok(reservations);
    }

    [HttpGet("my-reservations")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = GetCurrentUserId();
        var reservations = await _reservationService.GetReservationsByUserAsync(userId);
        return Ok(reservations);
    }

    [HttpGet("hotel/{hotelId}")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetReservationsByHotel(int hotelId)
    {
        var reservations = await _reservationService.GetReservationsByHotelAsync(hotelId);
        return Ok(reservations);
    }

    [HttpGet("date-range")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetReservationsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var reservations = await _reservationService.GetReservationsByDateRangeAsync(startDate, endDate);
        return Ok(reservations);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }

    private string GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? "";
    }
}