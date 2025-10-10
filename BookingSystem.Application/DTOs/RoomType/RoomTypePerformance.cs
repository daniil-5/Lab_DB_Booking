namespace BookingSystem.Application.DTOs.RoomType;

public class RoomTypePerformance
{
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; }
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AveragePrice { get; set; }
    public int ConfirmedCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal CancellationRate { get; set; }
}