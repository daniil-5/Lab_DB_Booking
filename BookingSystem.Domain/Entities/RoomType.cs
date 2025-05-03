namespace BookingSystem.Domain.Entities;

public class RoomType : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int BedCount { get; set; }
    public decimal Area { get; set; }
    public int? Floor { get; set; }
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}