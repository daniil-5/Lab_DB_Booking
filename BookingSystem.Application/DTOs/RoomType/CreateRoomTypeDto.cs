namespace BookingSystem.Application.DTOs.RoomType;

public class CreateRoomTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int? Floor { get; set; }
    public int HotelId { get; set; }
}