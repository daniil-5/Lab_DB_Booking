namespace BookingSystem.Application.DTOs.Hotel;

public class HotelRanking
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal BasePrice { get; set; }
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int RankInLocation { get; set; }
    public int OverallRevenueRank { get; set; }
    public decimal MarketShareInLocation { get; set; }
}