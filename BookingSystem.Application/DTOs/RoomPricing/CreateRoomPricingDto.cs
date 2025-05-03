namespace BookingSystem.Application.DTOs.RoomPricing;

public class CreateRoomPricingDto
{
    public int RoomId { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
}