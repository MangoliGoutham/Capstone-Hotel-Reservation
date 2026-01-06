using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelReservationSystem.DTOs;
using HotelReservationSystem.Services;
using Microsoft.Extensions.Logging;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;
    private readonly IReservationService _reservationService;
    private readonly ILogger<BillsController> _logger;

    public BillsController(IBillService billService, IReservationService reservationService, ILogger<BillsController> logger)
    {
        _billService = billService;
        _reservationService = reservationService;
        _logger = logger;
    }

   //get

    [HttpGet]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetAllBills()
    {
        var bills = await _billService.GetAllBillsAsync();
        return Ok(bills);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> GetBillById(int id)
    {
        var bill = await _billService.GetBillByIdAsync(id);
        if (bill == null)
        {
            return NotFound();
        }
        return Ok(bill);
    }

    [HttpGet("reservation/{reservationId}")]
    public async Task<IActionResult> GetBillByReservationId(int reservationId)
    {

        
        var bill = await _billService.GetBillByReservationIdAsync(reservationId);
        if (bill != null)
        {

             return Ok(bill);
        }

       
        
        
        var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
        
        if (reservation == null)
        {

             _logger.LogError($"[GetBillByReservationId] Reservation NOT found for ReservationId: {reservationId}");
             return NotFound(new { message = $"Reservation {reservationId} not found in database." });
        }



      
        try 
        {
             _logger.LogInformation($"[GetBillByReservationId] Auto-generating bill for reservation {reservationId} (Status: {reservation.Status})");
             var newBill = await _billService.CreateBillForReservationAsync(reservationId);

             return Ok(newBill);
        }
        catch (Exception ex)
        {

             _logger.LogError(ex, $"[GetBillByReservationId] Failed to auto-generate bill for reservation {reservationId}");
             
             
             bill = await _billService.GetBillByReservationIdAsync(reservationId);
             if (bill != null) return Ok(bill);

             return StatusCode(500, new { message = "Error generating bill", error = ex.Message });
        }
    }

    [HttpPatch("{id}/payment-status")]
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto updateDto)
    {
        var bill = await _billService.UpdatePaymentStatusAsync(id, updateDto);
        if (bill == null)
        {
            return NotFound();
        }
        return Ok(bill);
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayBill(int id)
    {
        var bill = await _billService.GetBillByIdAsync(id);
        if (bill == null) return NotFound();
        
        var updateDto = new UpdatePaymentStatusDto { PaymentStatus = "Paid" };
        var updatedBill = await _billService.UpdatePaymentStatusAsync(id, updateDto); 
        
       
        if (updatedBill != null)
        {
             await _reservationService.UpdateReservationStatusAsync(bill.ReservationId, 
                 new UpdateReservationStatusDto { Status = "Paid" });
        }
        
        return Ok(updatedBill);
    }
}