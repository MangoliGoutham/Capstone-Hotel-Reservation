using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int Capacity { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
}

public class CreateRoomDto
{
    [Required]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    public string RoomType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Required]
    [Range(1, 10)]
    public int Capacity { get; set; }

    public string? Description { get; set; }

    [Required]
    public int HotelId { get; set; }
}

public class RoomSearchDto
{
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int? HotelId { get; set; }
    public string? RoomType { get; set; }
}

public class UpdateRoomStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}