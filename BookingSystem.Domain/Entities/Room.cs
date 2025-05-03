namespace BookingSystem.Domain.Entities;

public class Room : BaseEntity
{
    public string RoomNumber { get; set; }
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<RoomPricing> Pricing { get; set; } = new List<RoomPricing>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}