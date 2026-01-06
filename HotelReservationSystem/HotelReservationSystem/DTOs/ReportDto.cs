namespace HotelReservationSystem.DTOs;

public class OccupancyReportDto
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal OccupancyRate { get; set; }
    public DateTime Date { get; set; }
}

public class RevenueReportDto
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalReservations { get; set; }
    public decimal AverageRevenuePerReservation { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ReservationSummaryDto
{
    public DateTime Date { get; set; }
    public int TotalReservations { get; set; }
    public int CheckedIn { get; set; }
    public int CheckedOut { get; set; }
    public int Cancelled { get; set; }
    public decimal TotalRevenue { get; set; }
}