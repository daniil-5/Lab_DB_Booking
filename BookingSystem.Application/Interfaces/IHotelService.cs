using BookingSystem.Application.Dtos.Hotel;
using BookingSystem.Application.DTOs.Hotel;
using BookingSystem.Application.Hotel;

namespace BookingSystem.Application.Interfaces;

public interface IHotelService
{
    Task<HotelDto> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<HotelDto> UpdateHotelAsync(UpdateHotelDto hotelDto);
    Task DeleteHotelAsync(int id);
    Task<HotelDto> GetHotelByIdAsync(int id);
    Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
    
    Task<HotelSearchResultDto> SearchHotelsAsync(HotelSearchDto searchDto);
}