using BookingSystem.Application.DTOs.Room;

namespace BookingSystem.Application.Interfaces;

public interface IRoomService
{
    Task<RoomDto> CreateRoomAsync(CreateRoomDto roomDto);
    Task<RoomDto> UpdateRoomAsync(UpdateRoomDto roomDto);
    Task DeleteRoomAsync(int id);
    Task<RoomDto> GetRoomByIdAsync(int id);
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
}