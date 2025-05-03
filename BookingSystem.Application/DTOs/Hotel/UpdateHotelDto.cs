using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Hotel;

public class UpdateHotelDto: CreateHotelDto
{
    [Required]
    public int Id { get; set; }
}