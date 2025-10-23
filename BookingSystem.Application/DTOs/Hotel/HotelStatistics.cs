namespace BookingSystem.Application.DTOs.Hotel;

public class HotelStatistics
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingPrice { get; set; }
    public int TotalRoomTypes { get; set; }
    public int TotalPhotos { get; set; }
}