using BookingSystem.Application.DTOs.Room;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _roomRepository;

        public RoomService(IRepository<Room> roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto roomDto)
        {
            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                RoomTypeId = roomDto.RoomTypeId,
                IsAvailable = roomDto.IsAvailable
            };

            await _roomRepository.AddAsync(room);
            return MapToDto(room);
        }

        public async Task<RoomDto> UpdateRoomAsync(UpdateRoomDto roomDto)
        {
            var existingRoom = await _roomRepository.GetByIdAsync(roomDto.Id);
            
            existingRoom.RoomNumber = roomDto.RoomNumber;
            existingRoom.RoomTypeId = roomDto.RoomTypeId;
            existingRoom.IsAvailable = roomDto.IsAvailable;

            await _roomRepository.UpdateAsync(existingRoom);
            return MapToDto(existingRoom);
        }

        public async Task DeleteRoomAsync(int id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        public async Task<RoomDto> GetRoomByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            return room != null ? MapToDto(room) : null;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(MapToDto).ToList();
        }

        private static RoomDto MapToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomTypeId = room.RoomTypeId,
                IsAvailable = room.IsAvailable,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }
    }