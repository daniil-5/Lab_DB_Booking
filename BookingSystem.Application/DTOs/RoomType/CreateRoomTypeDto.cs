namespace BookingSystem.Application.RoomType;

public class CreateRoomTypeDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Area { get; set; }
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int? Floor { get; set; }
    public int HotelId { get; set; }
}