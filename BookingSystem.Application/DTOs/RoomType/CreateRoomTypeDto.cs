namespace BookingSystem.Application.RoomType;

public class CreateRoomTypeDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int BedCount { get; set; }
    public decimal Area { get; set; }
    public int? Floor { get; set; }
    public int HotelId { get; set; }
}