using BookingSystem.Application.DTOs.RoomPricing;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class RoomPricingService : IRoomPricingService
{
    private readonly IRepository<RoomPricing> _roomPricingRepository;

    public RoomPricingService(IRepository<RoomPricing> roomPricingRepository)
    {
        _roomPricingRepository = roomPricingRepository;
    }

    public async Task<RoomPricingDto> CreateRoomPricingAsync(CreateRoomPricingDto pricingDto)
    {
        var roomPricing = new RoomPricing
        {
            RoomId = pricingDto.RoomId,
            Date = pricingDto.Date,
            Price = pricingDto.Price
        };

        await _roomPricingRepository.AddAsync(roomPricing);
        return MapToDto(roomPricing);
    }

    public async Task<RoomPricingDto> UpdateRoomPricingAsync(UpdateRoomPricingDto pricingDto)
    {
        var existingPricing = await _roomPricingRepository.GetByIdAsync(pricingDto.Id);

        existingPricing.RoomId = pricingDto.RoomId;
        existingPricing.Date = pricingDto.Date;
        existingPricing.Price = pricingDto.Price;

        await _roomPricingRepository.UpdateAsync(existingPricing);
        return MapToDto(existingPricing);
    }

    public async Task DeleteRoomPricingAsync(int id)
    {
        await _roomPricingRepository.DeleteAsync(id);
    }

    public async Task<RoomPricingDto> GetRoomPricingByIdAsync(int id)
    {
        var pricing = await _roomPricingRepository.GetByIdAsync(id);
        return pricing != null ? MapToDto(pricing) : null;
    }

    public async Task<IEnumerable<RoomPricingDto>> GetAllRoomPricingsAsync()
    {
        var pricings = await _roomPricingRepository.GetAllAsync();
        return pricings.Select(MapToDto).ToList();
    }

    private static RoomPricingDto MapToDto(RoomPricing pricing)
    {
        return new RoomPricingDto
        {
            Id = pricing.Id,
            RoomId = pricing.RoomId,
            Date = pricing.Date,
            Price = pricing.Price,
            CreatedAt = pricing.CreatedAt,
            UpdatedAt = pricing.UpdatedAt
        };
    }
}