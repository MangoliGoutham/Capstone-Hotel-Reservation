using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "General"; // Booking, CheckIn, CheckOut, General

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
