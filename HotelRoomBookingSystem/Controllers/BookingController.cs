using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services;
using HotelRoomBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            AppDbContext context,
            IStripeService stripeService,
            ILogger<BookingController> logger)
        {
            _context = context;
            _stripeService = stripeService;
            _logger = logger;
        }

        // ================= CREATE (GET) =================
        [HttpGet]
        public async Task<IActionResult> Create(int roomId, DateTime? checkIn, DateTime? checkOut)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null || !room.IsAvailable)
                {
                    TempData["ErrorMessage"] = "Room not available.";
                    return RedirectToAction("Availability", "Room");
                }

                if (!checkIn.HasValue || !checkOut.HasValue || checkIn >= checkOut)
                {
                    TempData["ErrorMessage"] = "Please select valid dates.";
                    return RedirectToAction("Availability", "Room");
                }

                var isAvailable = await IsRoomAvailableAsync(roomId, checkIn.Value, checkOut.Value);
                if (!isAvailable)
                {
                    TempData["ErrorMessage"] = "Room not available for selected dates.";
                    return RedirectToAction("Availability", "Room");
                }

                var booking = new Booking
                {
                    RoomId = roomId,
                    CheckInDate = checkIn.Value,
                    CheckOutDate = checkOut.Value,
                    Room = room
                };

                booking.TotalAmount =
                    (booking.CheckOutDate - booking.CheckInDate).Days * room.PricePerNight;

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create booking GET failed");
                TempData["ErrorMessage"] = "Error creating booking.";
                return RedirectToAction("Availability", "Room");
            }
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    booking.Room = await _context.Rooms.FindAsync(booking.RoomId);
                    if (booking.Room != null)
                    {
                        booking.TotalAmount =
                            (booking.CheckOutDate - booking.CheckInDate).Days * booking.Room.PricePerNight;
                    }
                    return View(booking);
                }

                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room == null || !room.IsAvailable)
                {
                    TempData["ErrorMessage"] = "Room not available.";
                    return RedirectToAction("Availability", "Room");
                }

                var isAvailable = await IsRoomAvailableAsync(
                    booking.RoomId, booking.CheckInDate, booking.CheckOutDate);

                if (!isAvailable)
                {
                    TempData["ErrorMessage"] = "Room not available for selected dates.";
                    return RedirectToAction("Availability", "Room");
                }

                booking.TotalAmount =
                    (booking.CheckOutDate - booking.CheckInDate).Days * room.PricePerNight;

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction("Payment", new { bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create booking POST failed");
                TempData["ErrorMessage"] = "Error saving booking.";

                booking.Room = await _context.Rooms.FindAsync(booking.RoomId);
                if (booking.Room != null)
                {
                    booking.TotalAmount =
                        (booking.CheckOutDate - booking.CheckInDate).Days * booking.Room.PricePerNight;
                }

                return View(booking);
            }
        }

        // ================= PAYMENT =================
        public async Task<IActionResult> Payment(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment load failed");
                TempData["ErrorMessage"] = "Error loading payment page.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Details failed");
                TempData["ErrorMessage"] = "Error loading booking details.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ================= AVAILABILITY CHECK =================
        private async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                (
                    (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                    (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                    (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                ));
        }
    }
}

