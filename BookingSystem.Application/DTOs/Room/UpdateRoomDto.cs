using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.Room;

public class UpdateRoomDto: CreateRoomDto
{
    [Required]
    public int Id { get; set; }
}