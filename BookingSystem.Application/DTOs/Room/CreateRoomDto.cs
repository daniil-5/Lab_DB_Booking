using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.Room;

public class CreateRoomDto
{
    [Required]
    [StringLength(10)]
    public string RoomNumber { get; set; }

    [Required]
    public int RoomTypeId { get; set; }

    [Required]
    public bool IsAvailable { get; set; } = true;
}