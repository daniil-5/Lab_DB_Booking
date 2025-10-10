using BookingSystem.Application.DTOs.Booking;

namespace BookingSystem.Application.DTOs.User;

public class UserBookingHistory
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageBookingValue { get; set; }
    public DateTime? FirstBookingDate { get; set; }
    public DateTime? LastBookingDate { get; set; }
    public int UniqueHotelsVisited { get; set; }
    public List<UserRecentBooking> RecentBookings { get; set; }
    public List<LocationStatistic> FavoriteLocations { get; set; }
}