using HotelBookingSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelRoomBookingSystem.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Type { get; set; } = "Single";   // used in revenue report

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        public int MaxGuests { get; set; } = 2;

        public bool IsAvailable { get; set; } = true;

        public string? ImagePath { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        [NotMapped]
        public string ImageUrl => !string.IsNullOrEmpty(ImagePath)
            ? ImagePath
            : "/images/rooms/default.jpg";
    }
}


