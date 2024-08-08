using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);

        IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilteredList = "");

        Booking GetBookingById(int id);

        void UpdateStatus(int bookingId, string bookingStatus, int villanumber);

        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}