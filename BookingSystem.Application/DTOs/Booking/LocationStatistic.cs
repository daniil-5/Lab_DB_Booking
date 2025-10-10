namespace BookingSystem.Application.DTOs.Booking;

public class LocationStatistic
{
    public string Location { get; set; }
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
}