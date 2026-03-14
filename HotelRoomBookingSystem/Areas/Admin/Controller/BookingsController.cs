using HotelBookingSystem.Data;
using HotelRoomBookingSystem.Data;
using HotelRoomBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelRoomBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(AppDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.BookingStatus = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.PaymentStatus = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string? reason)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.BookingStatus = BookingStatus.Cancelled;

            if (!string.IsNullOrEmpty(reason))
                booking.SpecialRequests += "\nCancel: " + reason;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CheckInList()
        {
            var today = DateTime.Today;

            var list = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.CheckInDate.Date == today &&
                            b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> CheckOutList()
        {
            var today = DateTime.Today;

            var list = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.CheckOutDate.Date == today &&
                            b.BookingStatus == BookingStatus.CheckedIn)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> RevenueReport()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .ToListAsync();

            ViewBag.TotalRevenue = bookings.Sum(b => b.TotalAmount);
            return View(bookings);
        }
    }
}
