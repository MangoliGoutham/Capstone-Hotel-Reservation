using HotelReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace HotelReservationSystem.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(HotelDbContext context)
    {
        // 1. Seed Hotels First
        if (!await context.Hotels.AnyAsync())
        {
            var hotels = new List<Hotel>
            {
                new Hotel { Name = "Grand Plaza Hotel", Address = "123 Main Street", City = "New York", Country = "USA", PhoneNumber = "+1234567800", Email = "info@grandplaza.com", StarRating = 5 },
                new Hotel { Name = "City Center Inn", Address = "456 Downtown Ave", City = "Los Angeles", Country = "USA", PhoneNumber = "+1234567801", Email = "info@citycenter.com", StarRating = 4 },
                new Hotel { Name = "Seaside Resort", Address = "789 Beach Road", City = "Miami", Country = "USA", PhoneNumber = "+1234567802", Email = "info@seaside.com", StarRating = 4 }
            };
            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users (Now we can assign HotelId)
        if (!await context.Users.AnyAsync())
        {
            var firstHotel = await context.Hotels.FirstOrDefaultAsync();
            var hotelId = firstHotel?.Id;

            var users = new List<User>
            {
                new User { FirstName = "Admin", LastName = "User", Email = "admin@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin", PhoneNumber = "+1234567890" },
                // Assign Manager to First Hotel
                new User { FirstName = "Hotel", LastName = "Manager", Email = "manager@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), Role = "HotelManager", PhoneNumber = "+1234567891", HotelId = hotelId },
                // Assign Receptionist to First Hotel
                new User { FirstName = "Front", LastName = "Desk", Email = "receptionist@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("receptionist123"), Role = "Receptionist", PhoneNumber = "+1234567892", HotelId = hotelId },
                
                new User { FirstName = "John", LastName = "Doe", Email = "john.doe@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"), Role = "Guest", PhoneNumber = "+1234567893" },
                new User { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"), Role = "Guest", PhoneNumber = "+1234567894" }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // Backfill HotelId for existing Manager/Receptionist if missing
        var existingManager = await context.Users.FirstOrDefaultAsync(u => u.Role == "HotelManager" && u.HotelId == null);
        var existingReceptionist = await context.Users.FirstOrDefaultAsync(u => u.Role == "Receptionist" && u.HotelId == null);
        var defaultHotel = await context.Hotels.FirstOrDefaultAsync();

        if (defaultHotel != null)
        {
            if (existingManager != null)
            {
                existingManager.HotelId = defaultHotel.Id;
                context.Users.Update(existingManager);
            }
            if (existingReceptionist != null)
            {
                existingReceptionist.HotelId = defaultHotel.Id;
                context.Users.Update(existingReceptionist);
            }
            if (existingManager != null || existingReceptionist != null)
            {
                await context.SaveChangesAsync();
            }
        }

        if (!await context.Rooms.AnyAsync())
        {
            var rooms = new List<Room>();
            var hotels = await context.Hotels.ToListAsync();
            var roomTypes = new[] { "Standard", "Deluxe", "Suite", "Presidential" };
            var basePrices = new[] { 100m, 150m, 250m, 500m };

            foreach (var hotel in hotels)
            {
                for (int floor = 1; floor <= 3; floor++)
                {
                    for (int roomNum = 1; roomNum <= 10; roomNum++)
                    {
                        var typeIndex = (roomNum - 1) % 4;
                        rooms.Add(new Room
                        {
                            RoomNumber = $"{floor}{roomNum:D2}",
                            RoomType = roomTypes[typeIndex],
                            BasePrice = basePrices[typeIndex],
                            Capacity = typeIndex + 1,
                            Description = $"{roomTypes[typeIndex]} room with modern amenities",
                            Status = "Available",
                            HotelId = hotel.Id
                        });
                    }
                }
            }
            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
        }

        // Seed Reservations AND Bills
        if (!await context.Reservations.AnyAsync())
        {
            var users = await context.Users.ToListAsync(); // Fetch existing users
            var rooms = await context.Rooms.ToListAsync(); // Fetch existing rooms
            var guestUsers = users.Where(u => u.Role == "Guest").ToList();
            var availableRooms = rooms.Take(5).ToList();
            var reservations = new List<Reservation>();

            if (guestUsers.Any() && availableRooms.Any())
            {
                for (int i = 0; i < 5; i++)
                {
                    var checkIn = DateTime.Today.AddDays(i + 1);
                    var checkOut = checkIn.AddDays(2);
                    var room = availableRooms[i];
                    var nights = (checkOut - checkIn).Days;

                    reservations.Add(new Reservation
                    {
                        ReservationNumber = $"RES{DateTime.Now.Ticks}{i}",
                        UserId = guestUsers[i % guestUsers.Count].Id,
                        RoomId = room.Id,
                        CheckInDate = checkIn,
                        CheckOutDate = checkOut,
                        NumberOfGuests = 2,
                        TotalAmount = room.BasePrice * nights,
                        Status = i < 2 ? "Confirmed" : "Booked",
                        SpecialRequests = i % 2 == 0 ? "Late check-in requested" : null
                    });
                }
                await context.Reservations.AddRangeAsync(reservations);
                await context.SaveChangesAsync();

                // Seed Bills for these reservations
                var bills = new List<Bill>();
                foreach (var reservation in reservations)
                {
                    var taxRate = 0.1m;
                    var taxAmount = reservation.TotalAmount * taxRate;
                    var totalAmount = reservation.TotalAmount + taxAmount;

                    bills.Add(new Bill
                    {
                        BillNumber = $"BILL{DateTime.Now.Ticks}{reservation.Id}",
                        ReservationId = reservation.Id,
                        RoomCharges = reservation.TotalAmount,
                        AdditionalCharges = 0,
                        TaxAmount = taxAmount,
                        TotalAmount = totalAmount,
                        PaymentStatus = reservation.Status == "Confirmed" ? "Paid" : "Pending"
                    });
                }
                await context.Bills.AddRangeAsync(bills);
                await context.SaveChangesAsync();
            }
        }

        // Seed Dashboard Test Data (Explicitly ensure today's data)
        var today = DateTime.Today;
        if (!await context.Reservations.AnyAsync(r => r.CheckInDate == today))
        {
             // Need to fetch fresh if not in local scope
            var user = await context.Users.FirstOrDefaultAsync(u => u.Role == "Guest");
            var room1 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "101");
            var room2 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "102");
            var room3 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "103");
            var room4 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "104");

            if (user != null && room1 != null && room2 != null && room3 != null && room4 != null)
            {
                var reservations = new List<Reservation>
                {
                    // Today's Arrival
                    new Reservation { ReservationNumber = $"RES-ARRIVE-{DateTime.Now.Ticks}", UserId = user.Id, RoomId = room1.Id, CheckInDate = today, CheckOutDate = today.AddDays(2), NumberOfGuests = 2, TotalAmount = 300, Status = "Confirmed", SpecialRequests = "Early check-in", CreatedAt = DateTime.UtcNow },
                    // Today's Departure
                    new Reservation { ReservationNumber = $"RES-DEPART-{DateTime.Now.Ticks}", UserId = user.Id, RoomId = room2.Id, CheckInDate = today.AddDays(-2), CheckOutDate = today, NumberOfGuests = 1, TotalAmount = 200, Status = "CheckedIn", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                    // Active
                    new Reservation { ReservationNumber = $"RES-ACTIVE-{DateTime.Now.Ticks}", UserId = user.Id, RoomId = room3.Id, CheckInDate = today.AddDays(-1), CheckOutDate = today.AddDays(3), NumberOfGuests = 2, TotalAmount = 600, Status = "CheckedIn", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    // Cancelled
                    new Reservation { ReservationNumber = $"RES-CX-{DateTime.Now.Ticks}", UserId = user.Id, RoomId = room4.Id, CheckInDate = today.AddDays(1), CheckOutDate = today.AddDays(3), NumberOfGuests = 2, TotalAmount = 300, Status = "Cancelled", CreatedAt = DateTime.UtcNow }
                };

                await context.Reservations.AddRangeAsync(reservations);
                await context.SaveChangesAsync();
            }
        }
    }
}