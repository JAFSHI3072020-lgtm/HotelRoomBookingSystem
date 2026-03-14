using HotelBookingSystem.Models;
using HotelRoomBookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= ROOM =================
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => r.RoomNumber).IsUnique();

                entity.Property(r => r.PricePerNight)
                      .HasPrecision(18, 2);

                entity.Property(r => r.RoomNumber)
                      .HasMaxLength(10);

                entity.Property(r => r.Description)
                      .HasMaxLength(500);
            });

            // ================= BOOKING =================
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.TotalAmount)
                      .HasPrecision(18, 2);

                entity.Property(b => b.AdvancePaid)
                      .HasPrecision(18, 2);

                entity.HasOne(b => b.Room)
                      .WithMany(r => r.Bookings)
                      .HasForeignKey(b => b.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
