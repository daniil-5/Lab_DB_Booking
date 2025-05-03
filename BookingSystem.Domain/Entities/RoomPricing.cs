namespace BookingSystem.Domain.Entities;

public class RoomPricing : BaseEntity
{
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
}