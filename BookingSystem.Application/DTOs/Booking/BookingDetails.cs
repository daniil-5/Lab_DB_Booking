namespace BookingSystem.Application.DTOs.Booking;

public class BookingDetails
{
    public int BookingId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int GuestCount { get; set; }
    public int Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int NightCount { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string UserEmail { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; }
    public string HotelLocation { get; set; }
    public decimal HotelRating { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; }
    public int RoomCapacity { get; set; }
    public decimal RoomArea { get; set; }
    public decimal RoomBasePrice { get; set; }
    public string RoomAmenities { get; set; }
    public string MainPhotoUrl { get; set; }
}