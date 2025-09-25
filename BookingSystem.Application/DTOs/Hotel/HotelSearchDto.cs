namespace BookingSystem.Application.DTOs.Hotel;

public class HotelSearchDto
{
    // Search criteria
    public string? Name { get; set; }
    public string? Location { get; set; }
    public decimal? MinRating { get; set; }
    public decimal? MaxRating { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? RoomTypeId { get; set; }
    public ICollection<string>? Amenities { get; set; }
        
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
        
    // Sorting
    public string SortBy { get; set; } = "Rating"; // Rating, Name, Location, Price
    public bool SortDescending { get; set; } = true; // Default to highest rating first
}