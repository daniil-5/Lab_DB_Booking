namespace BookingSystem.Application.DTOs.Room;

public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; }
    public int RoomTypeId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}