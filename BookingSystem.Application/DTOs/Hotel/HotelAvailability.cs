namespace BookingSystem.Application.DTOs.Hotel;

public class HotelAvailability
{
    public int HotelId { get; set; }
    public string HotelName { get; set; }
    public string Location { get; set; }
    public decimal Rating { get; set; }
    public string Description { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int PhotoCount { get; set; }
    public int BookedCount { get; set; }
}