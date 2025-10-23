namespace BookingSystem.Application.DTOs.Booking;

public class MonthlyBookingTrend
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public DateTime Month { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int UniqueCustomers { get; set; }
    public int TotalGuests { get; set; }
}