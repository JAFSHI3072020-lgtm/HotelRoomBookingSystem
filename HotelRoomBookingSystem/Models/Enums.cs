namespace HotelRoomBookingSystem.Models
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        CheckedIn = 2,
        CheckedOut = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        PartiallyPaid = 1,
        Paid = 2,
        Refunded = 3
    }
}
