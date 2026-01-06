using HotelReservationSystem.DTOs;
using HotelReservationSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Services;

public interface IReportService
{
    Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync(DateTime date, int? hotelId = null);
    Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, int? hotelId = null);
    Task<IEnumerable<ReservationSummaryDto>> GetReservationSummaryAsync(DateTime startDate, DateTime endDate, int? hotelId = null);
}

public class ReportService : IReportService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IBillRepository _billRepository;

    public ReportService(
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository,
        IHotelRepository hotelRepository,
        IBillRepository billRepository)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _hotelRepository = hotelRepository;
        _billRepository = billRepository;
    }

    public async Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync(DateTime date, int? hotelId = null)
    {
        var hotels = await _hotelRepository.GetAllAsync();
        if (hotelId.HasValue)
        {
            hotels = hotels.Where(h => h.Id == hotelId.Value);
        }

        var occupancyReports = new List<OccupancyReportDto>();

        foreach (var hotel in hotels)
        {
            var totalRooms = (await _roomRepository.GetRoomsByHotelAsync(hotel.Id)).Count();
            
            var occupiedRooms = (await _reservationRepository.FindAsync(r => 
                r.Room.HotelId == hotel.Id &&
                r.Status != "Cancelled" &&
                date >= r.CheckInDate &&
                date < r.CheckOutDate)).Count();

            var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

            occupancyReports.Add(new OccupancyReportDto
            {
                HotelId = hotel.Id,
                HotelName = hotel.Name,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = Math.Round(occupancyRate, 2),
                Date = date
            });
        }

        return occupancyReports;
    }

    public async Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, int? hotelId = null)
    {
        var hotels = await _hotelRepository.GetAllAsync();
        if (hotelId.HasValue)
        {
            hotels = hotels.Where(h => h.Id == hotelId.Value);
        }

        var revenueReports = new List<RevenueReportDto>();

        foreach (var hotel in hotels)
        {
            var reservations = await _reservationRepository.FindAsync(r => 
                r.Room.HotelId == hotel.Id &&
                r.Status != "Cancelled" &&
                r.CheckInDate >= startDate &&
                r.CheckInDate <= endDate);

            var totalRevenue = reservations.Sum(r => r.TotalAmount);
            var totalReservations = reservations.Count();
            var averageRevenue = totalReservations > 0 ? totalRevenue / totalReservations : 0;

            revenueReports.Add(new RevenueReportDto
            {
                HotelId = hotel.Id,
                HotelName = hotel.Name,
                TotalRevenue = totalRevenue,
                TotalReservations = totalReservations,
                AverageRevenuePerReservation = Math.Round(averageRevenue, 2),
                StartDate = startDate,
                EndDate = endDate
            });
        }

        return revenueReports;
    }

    public async Task<IEnumerable<ReservationSummaryDto>> GetReservationSummaryAsync(DateTime startDate, DateTime endDate, int? hotelId = null)
    {
        var summaries = new List<ReservationSummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayReservations = await _reservationRepository.FindAsync(r => 
                r.CheckInDate.Date == currentDate.Date && 
                (!hotelId.HasValue || r.Room.HotelId == hotelId.Value));

            var totalReservations = dayReservations.Count();
            var checkedIn = dayReservations.Count(r => r.Status == "Checked-in");
            var checkedOut = dayReservations.Count(r => r.Status == "Checked-out");
            var cancelled = dayReservations.Count(r => r.Status == "Cancelled");
            var totalRevenue = dayReservations.Where(r => r.Status != "Cancelled").Sum(r => r.TotalAmount);

            summaries.Add(new ReservationSummaryDto
            {
                Date = currentDate,
                TotalReservations = totalReservations,
                CheckedIn = checkedIn,
                CheckedOut = checkedOut,
                Cancelled = cancelled,
                TotalRevenue = totalRevenue
            });

            currentDate = currentDate.AddDays(1);
        }

        return summaries;
    }
}