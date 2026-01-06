using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
}

public class CreateReservationDto
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Range(1, 10)]
    public int NumberOfGuests { get; set; }

    public string? SpecialRequests { get; set; }
}

public class UpdateReservationStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}