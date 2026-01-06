using HotelReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace HotelReservationSystem.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(HotelDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // seed users
        var users = new List<User>
        {
            new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@hotel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                PhoneNumber = "+1234567890"
            },
            new User
            {
                FirstName = "Hotel",
                LastName = "Manager",
                Email = "manager@hotel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                Role = "HotelManager",
                PhoneNumber = "+1234567891"
            },
            new User
            {
                FirstName = "Front",
                LastName = "Desk",
                Email = "receptionist@hotel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("receptionist123"),
                Role = "Receptionist",
                PhoneNumber = "+1234567892"
            },
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
                Role = "Guest",
                PhoneNumber = "+1234567893"
            },
            new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
                Role = "Guest",
                PhoneNumber = "+1234567894"
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // seed hotels
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Name = "Grand Plaza Hotel",
                Address = "123 Main Street",
                City = "New York",
                Country = "USA",
                PhoneNumber = "+1234567800",
                Email = "info@grandplaza.com",
                StarRating = 5
            },
            new Hotel
            {
                Name = "City Center Inn",
                Address = "456 Downtown Ave",
                City = "Los Angeles",
                Country = "USA",
                PhoneNumber = "+1234567801",
                Email = "info@citycenter.com",
                StarRating = 4
            },
            new Hotel
            {
                Name = "Seaside Resort",
                Address = "789 Beach Road",
                City = "Miami",
                Country = "USA",
                PhoneNumber = "+1234567802",
                Email = "info@seaside.com",
                StarRating = 4
            }
        };

        await context.Hotels.AddRangeAsync(hotels);
        await context.SaveChangesAsync();

        // seed rooms
        var rooms = new List<Room>();
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

        // seed reservations
        var guestUsers = users.Where(u => u.Role == "Guest").ToList();
        var availableRooms = rooms.Take(5).ToList();
        var reservations = new List<Reservation>();

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

        // seed bills
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