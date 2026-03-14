using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        // Profile picture
        public string? ProfilePicture { get; set; } = "/images/default-avatar.jpg";
    }
}