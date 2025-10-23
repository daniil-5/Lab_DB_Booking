using BookingSystem.Application.DTOs.RoomType;

namespace BookingSystem.Application.DTOs.Hotel;

public class HotelPerformanceReport
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int UniqueCustomers { get; set; }
    public int TotalRoomTypes { get; set; }
    public List<RoomTypePerformance> RoomTypePerformance { get; set; } = new List<RoomTypePerformance>();
}