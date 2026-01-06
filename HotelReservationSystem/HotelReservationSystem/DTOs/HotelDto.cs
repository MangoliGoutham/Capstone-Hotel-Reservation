using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.DTOs;

public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int StarRating { get; set; }
}

public class CreateHotelDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    [Range(1, 5)]
    public int StarRating { get; set; }
}