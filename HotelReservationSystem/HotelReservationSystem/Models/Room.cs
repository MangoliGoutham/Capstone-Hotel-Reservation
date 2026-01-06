using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationSystem.Models;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RoomType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal BasePrice { get; set; }

    public int Capacity { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Available";

    public int HotelId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("HotelId")]
    public virtual Hotel Hotel { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}