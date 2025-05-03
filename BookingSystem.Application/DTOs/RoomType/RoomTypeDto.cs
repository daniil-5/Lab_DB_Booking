namespace BookingSystem.Application.RoomType;

public class RoomTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BedCount { get; set; }
    public decimal Area { get; set; }
    public int? Floor { get; set; }
    public int HotelId { get; set; }
}