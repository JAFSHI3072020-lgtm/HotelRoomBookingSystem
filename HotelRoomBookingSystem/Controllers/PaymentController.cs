using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            AppDbContext context,
            IStripeService stripeService,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _stripeService = stripeService;
            _logger = logger;
        }

        // ================= START PAYMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int bookingId)
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

                // Redirect to Stripe Checkout
                var user = new ApplicationUser
                {
                    Id = "guest",
                    Email = booking.CustomerEmail,
                    FullName = booking.CustomerName
                };

                var session = await _stripeService.CreateCheckoutSessionAsync(booking, user);

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe payment process failed");
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Payment", "Booking", new { bookingId });
            }
        }

        // ================= PAYMENT SUCCESS =================
        public async Task<IActionResult> Success(string sessionId)
        {
            try
            {
                var isPaid = await _stripeService.VerifyPaymentAsync(sessionId);

                if (!isPaid)
                {
                    TempData["ErrorMessage"] = "Payment verification failed.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Message = "Payment successful!";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment success verification failed");
                TempData["ErrorMessage"] = "Payment verification error.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ================= PAYMENT CANCEL =================
        public IActionResult Cancel()
        {
            return View();
        }
    }
}
