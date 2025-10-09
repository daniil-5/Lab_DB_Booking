using System.Linq.Expressions;
using BookingSystem.Application.Dtos.Hotel;
using BookingSystem.Application.DTOs.Hotel;
using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;

    public HotelService(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var hotel = new Domain.Entities.Hotel
        {
            Name = hotelDto.Name,
            Description = hotelDto.Description,
            Location = hotelDto.Location,
            Rating = hotelDto.Rating,
            BasePrice = hotelDto.BasePrice,

            CreatedAt = DateTime.UtcNow
        };

        await _hotelRepository.AddAsync(hotel);
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
        existingHotel.Rating = hotelDto.Rating;
        existingHotel.BasePrice = hotelDto.BasePrice;

        existingHotel.UpdatedAt = DateTime.UtcNow;

        await _hotelRepository.UpdateAsync(existingHotel);
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
            Rating = hotel.Rating,
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
}
