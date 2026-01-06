using HotelReservationSystem.DTOs;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;

namespace HotelReservationSystem.Services;

public interface IBillService
{
    Task<IEnumerable<BillDto>> GetAllBillsAsync();
    Task<BillDto?> GetBillByIdAsync(int id);
    Task<BillDto?> GetBillByReservationIdAsync(int reservationId);
    Task<BillDto> CreateBillForReservationAsync(int reservationId);
    Task<BillDto?> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto updateDto);
}

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly INotificationQueue _notificationQueue;

    public BillService(
        IBillRepository billRepository, 
        IReservationRepository reservationRepository,
        INotificationQueue notificationQueue)
    {
        _billRepository = billRepository;
        _reservationRepository = reservationRepository;
        _notificationQueue = notificationQueue;
    }

    public async Task<IEnumerable<BillDto>> GetAllBillsAsync()
    {
        var bills = await _billRepository.GetAllAsync();
        var billDtos = new List<BillDto>();

        foreach (var bill in bills)
        {
            var reservation = await _reservationRepository.GetByIdAsync(bill.ReservationId);
            billDtos.Add(MapToDto(bill, reservation?.ReservationNumber ?? ""));
        }

        return billDtos;
    }

    public async Task<BillDto?> GetBillByIdAsync(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null) return null;

        var reservation = await _reservationRepository.GetByIdAsync(bill.ReservationId);
        return MapToDto(bill, reservation?.ReservationNumber ?? "");
    }

    public async Task<BillDto?> GetBillByReservationIdAsync(int reservationId)
    {
        var bill = await _billRepository.GetBillByReservationIdAsync(reservationId);
        if (bill == null) return null;

        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        return MapToDto(bill, reservation?.ReservationNumber ?? "");
    }

    public async Task<BillDto> CreateBillForReservationAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
            throw new ArgumentException("Reservation not found");

        var taxRate = 0.1m; // 10% tax
        var taxAmount = reservation.TotalAmount * taxRate;
        var totalAmount = reservation.TotalAmount + taxAmount;

        var bill = new Bill
        {
            BillNumber = GenerateBillNumber(),
            ReservationId = reservationId,
            RoomCharges = reservation.TotalAmount,
            AdditionalCharges = 0,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaymentStatus = "Pending"
        };

        await _billRepository.AddAsync(bill);

        // Queue Notification
        await _notificationQueue.QueueNotificationAsync(new Notification
        {
            UserId = reservation.UserId,
            Type = "Billing",
            Message = $"Bill #{bill.BillNumber} has been generated. Total: {bill.TotalAmount:C}."
        });

        return MapToDto(bill, reservation.ReservationNumber);
    }

    public async Task<BillDto?> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto updateDto)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null) return null;

        bill.PaymentStatus = updateDto.PaymentStatus;
        if (updateDto.PaymentStatus == "Paid")
        {
            bill.PaidAt = DateTime.UtcNow;
            
            // Also update the reservation status to Paid
            var reservation = await _reservationRepository.GetByIdAsync(bill.ReservationId);
            if (reservation != null)
            {
                reservation.Status = "Paid";
                await _reservationRepository.UpdateAsync(reservation);

                // Queue Notification
                await _notificationQueue.QueueNotificationAsync(new Notification
                {
                    UserId = reservation.UserId,
                    Type = "Payment",
                    Message = $"Payment successful for Bill #{bill.BillNumber}. Amount: {bill.TotalAmount:C}."
                });
            }
        }
        else 
        {
             await _billRepository.UpdateAsync(bill);
        }

        var res = await _reservationRepository.GetByIdAsync(bill.ReservationId);
        return MapToDto(bill, res?.ReservationNumber ?? "");
    }

    private static BillDto MapToDto(Bill bill, string reservationNumber)
    {
        return new BillDto
        {
            Id = bill.Id,
            BillNumber = bill.BillNumber,
            ReservationId = bill.ReservationId,
            RoomCharges = bill.RoomCharges,
            AdditionalCharges = bill.AdditionalCharges,
            TaxAmount = bill.TaxAmount,
            TotalAmount = bill.TotalAmount,
            PaymentStatus = bill.PaymentStatus,
            CreatedAt = bill.CreatedAt,
            PaidAt = bill.PaidAt,
            ReservationNumber = reservationNumber
        };
    }

    private static string GenerateBillNumber()
    {
        // Use a clearer and more unique format
        return $"BILL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}