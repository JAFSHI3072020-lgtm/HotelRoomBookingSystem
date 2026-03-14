using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelRoomBookingSystem.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        // ---------- RELATIONS ----------
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; } = null!;

        public string? UserId { get; set; }   // Admin panel uses User

        // ---------- CUSTOMER ----------
        [Required, StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        public string? SpecialRequests { get; set; }

        // ---------- DATES ----------
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        // ---------- STATUS ----------
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // ---------- MONEY ----------
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }
}




