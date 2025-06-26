using System.ComponentModel.DataAnnotations;
using BookingSystem.Application.DTOs.HotelPhoto;

namespace BookingSystem.Application.Hotel;

public class CreateHotelDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; }

    [Range(0, 5.0)]
    public decimal Rating { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }
    
    [Required]
    public ICollection<string> Amenities { get; set; } = new List<string>();
    
    [Required]
    public ICollection<Domain.Entities.RoomType> RoomTypes { get; set; } = new List<Domain.Entities.RoomType>();
    
    [Required]
    public ICollection<HotelPhotoDto> Photos { get; set; } = new List<HotelPhotoDto>();
}