namespace BookingSystem.Domain.DTOs.User;

public class UserRecentBooking
{
    public required int BookingId { get; set; }
    public required DateTime CheckInDate { get; set; }
    public required DateTime CheckOutDate { get; set; }
    public required int Status { get; set; }
    public required decimal TotalPrice { get; set; }
    public required string HotelName { get; set; }
    public required string Location { get; set; }
    public required string RoomTypeName { get; set; }
}