namespace HotelReservationSystem.DTOs;

public class BillDto
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public int ReservationId { get; set; }
    public decimal RoomCharges { get; set; }
    public decimal AdditionalCharges { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
}

public class UpdatePaymentStatusDto
{
    public string PaymentStatus { get; set; } = string.Empty;
}