using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var featuredRooms = await _context.Rooms
                    .Where(r => r.IsAvailable)
                    .OrderBy(r => r.PricePerNight)
                    .Take(3)
                    .ToListAsync();

                ViewBag.TotalRooms = await _context.Rooms.CountAsync();
                ViewBag.AvailableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);

                return View(featuredRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View(new List<Room>());
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}