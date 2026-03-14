using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingSystem.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Card"; // Card, Cash, Bank Transfer

        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}
