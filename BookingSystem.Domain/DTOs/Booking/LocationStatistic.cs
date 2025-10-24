namespace BookingSystem.Application.DTOs.Booking;

public class LocationStatistic
{
    public required string Location { get; set; } = string.Empty;
    public required int BookingCount { get; set; }
    public required decimal TotalSpent { get; set; }
}