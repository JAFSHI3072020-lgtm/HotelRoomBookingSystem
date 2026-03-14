using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelRoomBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingSystem.Controllers
{
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoomController> _logger;

        public RoomController(AppDbContext context, ILogger<RoomController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Room
        public async Task<IActionResult> Index(string? search, string? roomType, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                ViewBag.Search = search;
                ViewBag.RoomType = roomType;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;

                var query = _context.Rooms.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r =>
                        r.RoomNumber.Contains(search) ||
                        (!string.IsNullOrEmpty(r.Description) && r.Description.Contains(search)) ||
                        (!string.IsNullOrEmpty(r.RoomType) && r.RoomType.Contains(search)));
                }

                if (!string.IsNullOrEmpty(roomType) && roomType != "All")
                {
                    query = query.Where(r => r.RoomType == roomType);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(r => r.PricePerNight >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(r => r.PricePerNight <= maxPrice.Value);
                }

                var rooms = await query
                    .Where(r => r.IsAvailable)
                    .OrderBy(r => r.PricePerNight)
                    .ToListAsync();

                // Get unique room types for filter dropdown
                ViewBag.RoomTypes = await _context.Rooms
                    .Select(r => r.RoomType)
                    .Distinct()
                    .Where(rt => !string.IsNullOrEmpty(rt))
                    .ToListAsync();

                // Add "All" option to room types
                ViewBag.RoomTypes = new List<string> { "All" }.Concat(ViewBag.RoomTypes).ToList();

                return View(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rooms");
                TempData["ErrorMessage"] = "Error loading rooms. Please try again.";
                return View(new List<Room>());
            }
        }

        // GET: /Room/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (room == null)
                {
                    TempData["ErrorMessage"] = "Room not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading room details for id {Id}", id);
                TempData["ErrorMessage"] = "Error loading room details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Room/Availability
        [HttpGet]
        public async Task<IActionResult> Availability(DateTime? checkIn, DateTime? checkOut, string? roomType, int? guests)
        {
            try
            {
                // Set default values if not provided
                var today = DateTime.Today;
                var defaultCheckIn = checkIn ?? today;
                var defaultCheckOut = checkOut ?? today.AddDays(1);

                ViewBag.CheckIn = defaultCheckIn.ToString("yyyy-MM-dd");
                ViewBag.CheckOut = defaultCheckOut.ToString("yyyy-MM-dd");
                ViewBag.RoomType = roomType;
                ViewBag.Guests = guests;

                // Validate dates
                if (defaultCheckIn < today || defaultCheckOut <= defaultCheckIn)
                {
                    TempData["ErrorMessage"] = "Please select valid check-in and check-out dates.";

                    // Show all available rooms
                    var allRooms = await _context.Rooms
                        .Where(r => r.IsAvailable)
                        .OrderBy(r => r.PricePerNight)
                        .ToListAsync();

                    ViewBag.RoomTypes = await GetRoomTypesAsync();
                    return View(allRooms);
                }

                // Get booked room IDs for the selected dates
                var bookedRoomIds = await _context.Bookings
                    .Where(b => b.Status != "Cancelled" &&
                               ((defaultCheckIn >= b.CheckInDate && defaultCheckIn < b.CheckOutDate) ||
                                (defaultCheckOut > b.CheckInDate && defaultCheckOut <= b.CheckOutDate) ||
                                (defaultCheckIn <= b.CheckInDate && defaultCheckOut >= b.CheckOutDate)))
                    .Select(b => b.RoomId)
                    .Distinct()
                    .ToListAsync();

                // Get available rooms
                var query = _context.Rooms
                    .Where(r => r.IsAvailable && !bookedRoomIds.Contains(r.Id));

                // Apply additional filters
                if (!string.IsNullOrEmpty(roomType) && roomType != "All")
                {
                    query = query.Where(r => r.RoomType == roomType);
                }

                if (guests.HasValue && guests > 0)
                {
                    query = query.Where(r => r.MaxGuests >= guests.Value);
                }

                var availableRooms = await query
                    .OrderBy(r => r.PricePerNight)
                    .ToListAsync();

                ViewBag.RoomTypes = await GetRoomTypesAsync();

                return View(availableRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability");
                TempData["ErrorMessage"] = "Error checking room availability.";
                return View(new List<Room>());
            }
        }

        private async Task<List<string>> GetRoomTypesAsync()
        {
            var roomTypes = await _context.Rooms
                .Select(r => r.RoomType)
                .Distinct()
                .Where(rt => !string.IsNullOrEmpty(rt))
                .ToListAsync();

            return new List<string> { "All" }.Concat(roomTypes).ToList();
        }
    }
}