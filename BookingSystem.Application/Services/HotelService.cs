using System.Linq.Expressions;
using BookingSystem.Application.DTOs.Hotel;
using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;
using BookingSystem.Application.DTOs.Booking;
using BookingSystem.Application.DTOs.RoomType;

namespace BookingSystem.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IUserActionAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HotelService(IHotelRepository hotelRepository, IUserActionAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _hotelRepository = hotelRepository;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var hotel = new Domain.Entities.Hotel
        {
            Name = hotelDto.Name,
            Description = hotelDto.Description,
            Location = hotelDto.Location,
            Rating = (double)hotelDto.Rating,
            BasePrice = hotelDto.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        await _hotelRepository.AddAsync(hotel);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.HotelCreated, true);

        return MapToDto(hotel);
    }

    public async Task<HotelDto> UpdateHotelAsync(UpdateHotelDto hotelDto)
    {
        var existingHotel = await _hotelRepository.GetByIdAsync(hotelDto.Id);
        if (existingHotel == null)
        {
            throw new KeyNotFoundException($"Hotel with ID {hotelDto.Id} not found");
        }

        existingHotel.Name = hotelDto.Name;
        existingHotel.Description = hotelDto.Description;
        existingHotel.Location = hotelDto.Location;
        existingHotel.Rating = (double)hotelDto.Rating;
        existingHotel.BasePrice = hotelDto.BasePrice;

        existingHotel.UpdatedAt = DateTime.UtcNow;

        await _hotelRepository.UpdateAsync(existingHotel);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.HotelUpdated, true);

        return MapToDto(existingHotel);
    }

    public async Task DeleteHotelAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null)
        {
            throw new KeyNotFoundException($"Hotel with ID {id} not found");
        }

        await _hotelRepository.DeleteAsync(id);

        var userId = GetCurrentUserId();
        await _auditService.AuditActionAsync(userId, UserActionType.HotelDeleted, true);
    }

    public async Task<HotelDto> GetHotelByIdAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);

        return hotel != null ? MapToDto(hotel) : null;
    }

    public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
    {
        var hotels = await _hotelRepository.GetAllAsync(
            predicate: h => !h.IsDeleted);

        return hotels.Select(MapToDto).ToList();
    }

    public async Task<HotelSearchResultDto> SearchHotelsAsync(HotelSearchDto searchDto)
    {
        var (hotels, totalCount) = await _hotelRepository.SearchHotelsAsync(null, null, searchDto.PageNumber, searchDto.PageSize);

        // Calculate pagination values
        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);
        var hasPrevious = searchDto.PageNumber > 1;
        var hasNext = searchDto.PageNumber < totalPages;

        // Map results to DTOs
        var hotelDtos = hotels.Select(MapToDto).ToList();

        // Create and return the search result DTO
        return new HotelSearchResultDto
        {
            Hotels = hotelDtos,
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages,
            HasPrevious = hasPrevious,
            HasNext = hasNext
        };
    }

    private static HotelDto MapToDto(Domain.Entities.Hotel hotel)
    {
        return new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Description = hotel.Description,
            Location = hotel.Location,
            Rating = (decimal)hotel.Rating,
            BasePrice = hotel.BasePrice,

            CreatedAt = hotel.CreatedAt,
            UpdatedAt = hotel.UpdatedAt,

            // Map RoomTypes (keeping as domain entities as defined in your DTO)
            RoomTypes = hotel.RoomTypes?
                .Where(rt => !rt.IsDeleted)
                .ToList() ?? new List<Domain.Entities.RoomType>(),

            // Map Photos
            Photos = hotel.Photos?
                .Where(p => !p.IsDeleted)
                .Select(p => new HotelPhotoDto
                {
                    Id = p.Id,
                    HotelId = p.HotelId,
                    Url = p.Url,
                    Description = p.Description,
                    IsMain = p.IsMain,
                    CreatedAt = p.CreatedAt
                })
                .ToList() ?? new List<HotelPhotoDto>()
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
    public async Task<IEnumerable<HotelStatistics>> GetHotelsStatisticsAsync()
    {
        var domainStatistics = await _hotelRepository.GetHotelsWithStatisticsAsync();
        return domainStatistics.Select(s => new HotelStatistics
        {
            HotelId = s.HotelId,
            HotelName = s.HotelName,
            Location = s.Location,
            Rating = s.Rating,
            BasePrice = s.BasePrice,
            TotalBookings = s.TotalBookings,
            ConfirmedBookings = s.ConfirmedBookings,
            CancelledBookings = s.CancelledBookings,
            TotalRevenue = s.TotalRevenue,
            AverageBookingPrice = s.AverageBookingPrice,
            TotalRoomTypes = s.TotalRoomTypes,
            TotalPhotos = s.TotalPhotos
        });
    }
    public async Task<IEnumerable<HotelAvailability>> SearchAvailableHotelsAsync(
        string location, 
        DateTime checkIn, 
        DateTime checkOut, 
        int guestCount)
    {
        if (checkOut <= checkIn)
        {
            throw new ArgumentException("Check-out date must be after check-in date");
        }
    
        if (guestCount <= 0)
        {
            throw new ArgumentException("Guest count must be greater than zero");
        }
    
        var domainAvailability = await _hotelRepository.SearchAvailableHotelsAsync(
            location, checkIn, checkOut, guestCount);

        return domainAvailability.Select(a => new HotelAvailability
        {
            HotelId = a.HotelId,
            HotelName = a.HotelName,
            Location = a.Location,
            Rating = a.Rating,
            Description = a.Description,
            RoomTypeId = a.RoomTypeId,
            RoomTypeName = a.RoomTypeName,
            Capacity = a.Capacity,
            Price = a.Price,
            Area = a.Area,
            PhotoCount = a.PhotoCount,
            BookedCount = a.BookedCount
        });
    }
    public async Task<IEnumerable<HotelRanking>> GetHotelsRankedByLocationAsync()
    {
        var domainRankings = await _hotelRepository.GetHotelsRankedByLocationAsync();
        return domainRankings.Select(r => new HotelRanking
        {
            HotelId = r.HotelId,
            HotelName = r.HotelName,
            Location = r.Location,
            Rating = r.Rating,
            BasePrice = r.BasePrice,
            BookingCount = r.BookingCount,
            TotalRevenue = r.TotalRevenue,
            RankInLocation = r.RankInLocation,
            OverallRevenueRank = r.OverallRevenueRank,
            MarketShareInLocation = r.MarketShareInLocation
        });
    }
    
    public async Task<HotelPerformanceReport> GetHotelPerformanceReportAsync(int hotelId)
    {
        var domainReport = await _hotelRepository.GetHotelPerformanceReportAsync(hotelId);
        
        if (domainReport == null)
        {
            throw new KeyNotFoundException($"Hotel with ID {hotelId} not found");
        }

        return new HotelPerformanceReport
        {
            HotelId = domainReport.HotelId,
            HotelName = domainReport.HotelName,
            Location = domainReport.Location,
            Rating = domainReport.Rating,
            BasePrice = domainReport.BasePrice,
            TotalBookings = domainReport.TotalBookings,
            TotalRevenue = domainReport.TotalRevenue,
            AverageBookingValue = domainReport.AverageBookingValue,
            UniqueCustomers = domainReport.UniqueCustomers,
            TotalRoomTypes = domainReport.TotalRoomTypes,
            RoomTypePerformance = domainReport.RoomTypePerformance.Select(p => new RoomTypePerformance
            {
                RoomTypeId = p.RoomTypeId,
                RoomTypeName = p.RoomTypeName,
                Capacity = p.Capacity,
                BasePrice = p.BasePrice,
                BookingCount = p.BookingCount,
                Revenue = p.Revenue,
                AveragePrice = p.AveragePrice,
                ConfirmedCount = p.ConfirmedCount,
                CancelledCount = p.CancelledCount,
                CancellationRate = p.CancellationRate
            }).ToList()
        };
    }
    
    public async Task<IEnumerable<MonthlyBookingTrend>> GetMonthlyBookingTrendsAsync(
        int? hotelId = null, 
        int months = 12)
    {
        if (months <= 0 || months > 24)
        {
            throw new ArgumentException("Months must be between 1 and 24");
        }
    
        var domainTrends = await _hotelRepository.GetMonthlyBookingTrendsAsync(hotelId, months);

        return domainTrends.Select(t => new MonthlyBookingTrend
        {
            HotelId = t.HotelId,
            HotelName = t.HotelName,
            Month = t.Month,
            BookingCount = t.BookingCount,
            Revenue = t.Revenue,
            AverageBookingValue = t.AverageBookingValue,
            UniqueCustomers = t.UniqueCustomers,
            TotalGuests = t.TotalGuests
        });
    }

    public async Task<IEnumerable<HotelDto>> GetHotelsOrderedByRatingAndNameAsync()
    {
        var hotels = await _hotelRepository.GetHotelsOrderedByRatingAndNameAsync();
        return hotels.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<HotelDto>> GetPremiumHotelsAsync()
    {
        var hotels = await _hotelRepository.GetPremiumHotelsAsync();
        return hotels.Select(MapToDto).ToList();
    }
}
