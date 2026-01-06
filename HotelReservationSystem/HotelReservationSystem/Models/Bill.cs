using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationSystem.Models;

public class Bill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string BillNumber { get; set; } = string.Empty;

    public int ReservationId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal RoomCharges { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal AdditionalCharges { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TaxAmount { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    [ForeignKey("ReservationId")]
    public virtual Reservation Reservation { get; set; } = null!;
}