using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.RoomType;
namespace BookingSystem.Application.Hotel;

public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public decimal Rating { get; set; }
    
    public decimal BasePrice { get; set; }
    
    public ICollection<Domain.Entities.RoomType> RoomTypes { get; set; } = new List<Domain.Entities.RoomType>();
    
    public ICollection<HotelPhotoDto> Photos { get; set; } = new List<HotelPhotoDto>();
    
    public ICollection<string> Amenities { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}