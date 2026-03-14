using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelRoomBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<RoomController> _logger;

        public RoomController(
            AppDbContext context,
            IWebHostEnvironment environment,
            ILogger<RoomController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // ================= LIST =================
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Bookings)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return View(rooms);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, IFormFile? roomImage)
        {
            if (!ModelState.IsValid)
                return View(room);

            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber))
            {
                ModelState.AddModelError("RoomNumber", "Room number already exists");
                return View(room);
            }

            if (roomImage != null)
                room.ImagePath = await UploadRoomImage(roomImage, room.RoomNumber);
            else
                room.ImagePath = "/images/rooms/default.jpg";

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room, IFormFile? roomImage)
        {
            if (id != room.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(room);

            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null) return NotFound();

            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.Type = room.Type;
            existingRoom.PricePerNight = room.PricePerNight;
            existingRoom.Description = room.Description;
            existingRoom.MaxGuests = room.MaxGuests;
            existingRoom.IsAvailable = room.IsAvailable;

            if (roomImage != null)
                existingRoom.ImagePath = await UploadRoomImage(roomImage, room.RoomNumber);

            _context.Update(existingRoom);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            // Only check date overlap (no Status field now)
            var hasActiveBookings = room.Bookings.Any(b =>
                b.CheckOutDate >= DateTime.Today);

            if (hasActiveBookings)
            {
                TempData["ErrorMessage"] = "Room has active bookings and cannot be disabled.";
                return RedirectToAction(nameof(Index));
            }

            room.IsAvailable = false;
            _context.Update(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= IMAGE UPLOAD =================
        private async Task<string> UploadRoomImage(IFormFile image, string roomNumber)
        {
            var fileName = $"{roomNumber}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "rooms");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/images/rooms/{fileName}";
        }
    }
}

