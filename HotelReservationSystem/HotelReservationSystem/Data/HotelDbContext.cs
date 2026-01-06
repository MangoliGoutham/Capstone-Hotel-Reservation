using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Notification> Notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasDefaultValue("Guest");
        });

       
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.Property(e => e.StarRating).HasDefaultValue(3);
        });

        
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(e => new { e.HotelId, e.RoomNumber }).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("Available");
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
        });

      
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasIndex(e => e.ReservationNumber).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("Booked");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
        });

       
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasIndex(e => e.BillNumber).IsUnique();
            entity.Property(e => e.PaymentStatus).HasDefaultValue("Pending");
            entity.Property(e => e.RoomCharges).HasPrecision(10, 2);
            entity.Property(e => e.AdditionalCharges).HasPrecision(10, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
        });

       
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Room)
            .WithMany(rm => rm.Reservations)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bill>()
            .HasOne(b => b.Reservation)
            .WithMany(r => r.Bills)
            .HasForeignKey(b => b.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}