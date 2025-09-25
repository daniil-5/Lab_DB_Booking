using BookingSystem.Application.DTOs.RoomType;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Services;

public class RoomTypeService : IRoomTypeService
    {
        private readonly IRepository<Domain.Entities.RoomType> _roomTypeRepository;
        private readonly IHotelRepository _hotelRepository;

        public RoomTypeService(
            IRepository<Domain.Entities.RoomType> roomTypeRepository,
            IHotelRepository hotelRepository)
        {
            _roomTypeRepository = roomTypeRepository;
            _hotelRepository = hotelRepository;
        }

        public async Task<RoomTypeDto> CreateRoomTypeAsync(CreateRoomTypeDto roomTypeDto)
        {
            // Verify hotel exists
            var hotel = await _hotelRepository.GetByIdAsync(roomTypeDto.HotelId);
            if (hotel == null)
            {
                throw new KeyNotFoundException($"Hotel with ID {roomTypeDto.HotelId} not found.");
            }

            var roomType = new Domain.Entities.RoomType
            {
                Name = roomTypeDto.Name,
                Description = roomTypeDto.Description,
                Capacity = roomTypeDto.Capacity,
                BasePrice = roomTypeDto.BasePrice,
                Area = roomTypeDto.Area,
                Floor = roomTypeDto.Floor,
                HotelId = roomTypeDto.HotelId
            };

            await _roomTypeRepository.AddAsync(roomType);
            return MapToDto(roomType);
        }

        public async Task<RoomTypeDto> UpdateRoomTypeAsync(UpdateRoomTypeDto roomTypeDto)
        {
            var hotel = await _hotelRepository.GetByIdAsync(roomTypeDto.HotelId);
            if (hotel == null)
            {
                throw new KeyNotFoundException($"Hotel with ID {roomTypeDto.HotelId} not found.");
            }
            
            var existingRoomType = await _roomTypeRepository.GetByIdAsync(roomTypeDto.Id);
            if (existingRoomType == null)
            {
                throw new KeyNotFoundException($"RoomType with ID {roomTypeDto.Id} not found.");
            }
            
            existingRoomType.Name = roomTypeDto.Name;
            existingRoomType.Description = roomTypeDto.Description;
            existingRoomType.Capacity = roomTypeDto.Capacity;
            existingRoomType.BasePrice = roomTypeDto.BasePrice;
            existingRoomType.Area = roomTypeDto.Area;
            existingRoomType.Floor = roomTypeDto.Floor;
            existingRoomType.HotelId = roomTypeDto.HotelId;

            await _roomTypeRepository.UpdateAsync(existingRoomType);
            return MapToDto(existingRoomType);
        }

        public async Task DeleteRoomTypeAsync(int id)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            if (roomType == null)
            {
                throw new KeyNotFoundException($"RoomType with ID {id} not found.");
            }
            
            // Check if there are any rooms of this type
            if (roomType.Rooms != null && roomType.Rooms.Any())
            {
                throw new InvalidOperationException("Cannot delete room type that is in use by rooms.");
            }
            
            await _roomTypeRepository.DeleteAsync(id);
        }

        public async Task<RoomTypeDto> GetRoomTypeByIdAsync(int id)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
                
            if (roomType == null)
            {
                return null;
            }
            
            return MapToDto(roomType);
        }

        public async Task<IEnumerable<RoomTypeDto>> GetAllRoomTypesAsync()
        {
            var roomTypes = await _roomTypeRepository.GetAllAsync();
            return roomTypes.Select(MapToDto).ToList();
        }
        public async Task<IEnumerable<RoomTypeDto>> GetRoomTypesByHotelIdAsync(int hotelId)
        {
            // First check if the hotel exists
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                throw new KeyNotFoundException($"Hotel with ID {hotelId} not found.");
            }
        
            // Get all room types for this hotel
            var roomTypes = await _roomTypeRepository.GetAllAsync(
                rt => rt.HotelId == hotelId && !rt.IsDeleted,
                include: query => query.Include(rt => rt.Rooms.Where(r => !r.IsDeleted)));
            
            return roomTypes.Select(MapToDto).ToList();
        }

        private static RoomTypeDto MapToDto(Domain.Entities.RoomType roomType)
        {
            return new RoomTypeDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                BasePrice = roomType.BasePrice,
                Capacity = roomType.Capacity,
                Area = roomType.Area,
                Floor = roomType.Floor,
                HotelId = roomType.HotelId
            };
        }
    }