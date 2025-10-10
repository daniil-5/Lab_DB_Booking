using BookingSystem.Application.DTOs.RoomPricing;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.Services;

public class RoomPricingService : IRoomPricingService
{
    private readonly IRepository<RoomPricing> _roomPricingRepository;
    private readonly IUserActionAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoomPricingService(IRepository<RoomPricing> roomPricingRepository, IUserActionAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _roomPricingRepository = roomPricingRepository;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RoomPricingDto> CreateRoomPricingAsync(CreateRoomPricingDto pricingDto)
    {
        var roomPricing = new RoomPricing
        {
            RoomTypeId = pricingDto.RoomTypeId,
            Date = pricingDto.Date,
            Price = pricingDto.Price
        };

        await _roomPricingRepository.AddAsync(roomPricing);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.RoomPricingsCreated, true);

        return MapToDto(roomPricing);
    }

    public async Task<RoomPricingDto> UpdateRoomPricingAsync(UpdateRoomPricingDto pricingDto)
    {
        var existingPricing = await _roomPricingRepository.GetByIdAsync(pricingDto.Id);

        existingPricing.RoomTypeId = pricingDto.RoomTypeId;
        existingPricing.Date = pricingDto.Date;
        existingPricing.Price = pricingDto.Price;

        await _roomPricingRepository.UpdateAsync(existingPricing);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.RoomPricingsUpdated, true);

        return MapToDto(existingPricing);
    }

    public async Task DeleteRoomPricingAsync(int id)
    {
        await _roomPricingRepository.DeleteAsync(id);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.RoomPricingsDeleted, true);
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
            RoomTypeId = pricing.RoomTypeId,
            Date = pricing.Date,
            Price = pricing.Price,
            CreatedAt = pricing.CreatedAt,
            UpdatedAt = pricing.UpdatedAt
        };
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        throw new InvalidOperationException("User ID not found in token");
    }
}