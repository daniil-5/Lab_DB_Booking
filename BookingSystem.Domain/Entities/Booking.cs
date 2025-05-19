using BookingSystem.Domain.Enums;
namespace BookingSystem.Domain.Entities;

public class Booking : BaseEntity
{
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public int UserId { get; set; }
    public Hotel Hotel  { get; set; }
    public int HotelId { get; set; }
    public User User { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; } = (int)BookingStatus.Pending;
}