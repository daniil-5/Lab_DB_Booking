using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.Hotel;

public class UpdateHotelDto: CreateHotelDto
{
    public int Id { get; set; }
    public new string Name { get; set; }
    public new string Description { get; set; }
    public new string Location { get; set; }
    public new decimal Rating { get; set; }
    
    public new decimal BasePrice { get; set; }
    
    public new ICollection<Domain.Entities.RoomType> RoomTypes { get; set; } = new List<Domain.Entities.RoomType>();
    
    public new ICollection<Domain.Entities.Hotel> Photos { get; set; } = new List<Domain.Entities.Hotel>();
    

}