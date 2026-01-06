using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelReservationSystem.Services;
using HotelReservationSystem.Repositories;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HotelManager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;

    public ReportsController(IReportService reportService, IUserRepository userRepository)
    {
        _reportService = reportService;
        _userRepository = userRepository;
    }

    private async Task<int?> GetUserHotelIdAsync()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return null;

        var user = await _userRepository.GetByEmailAsync(email);
        return user?.HotelId;
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyReport([FromQuery] DateTime date)
    {
        if (date == default) date = DateTime.Today;

        var hotelId = await GetUserHotelIdAsync();
        var report = await _reportService.GetOccupancyReportAsync(date, hotelId);
        return Ok(report);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate == default) startDate = DateTime.Today.AddDays(-30);
        if (endDate == default) endDate = DateTime.Today;

        var hotelId = await GetUserHotelIdAsync();
        var report = await _reportService.GetRevenueReportAsync(startDate, endDate, hotelId);
        return Ok(report);
    }

    [HttpGet("reservation-summary")]
    public async Task<IActionResult> GetReservationSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate == default) startDate = DateTime.Today.AddDays(-7);
        if (endDate == default) endDate = DateTime.Today;

        var hotelId = await GetUserHotelIdAsync();
        var report = await _reportService.GetReservationSummaryAsync(startDate, endDate, hotelId);
        return Ok(report);
    }
}