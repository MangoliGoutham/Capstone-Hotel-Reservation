using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationSystem.Models;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string ReservationNumber { get; set; } = string.Empty;

    public int UserId { get; set; }
    public int RoomId { get; set; }

    [Column(TypeName = "date")]
    public DateTime CheckInDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Booked";

    [StringLength(500)]
    public string? SpecialRequests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}