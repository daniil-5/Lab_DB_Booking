using System.ComponentModel.DataAnnotations;

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
}