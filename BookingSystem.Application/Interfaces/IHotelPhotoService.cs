using BookingSystem.Application.DTOs.HotelPhoto;

namespace BookingSystem.Application.Interfaces;

public interface IHotelPhotoService
{
    Task<HotelPhotoDto> CreateHotelPhotoAsync(CreateHotelPhotoDto photoDto);
    Task<HotelPhotoDto> UpdateHotelPhotoAsync(UpdateHotelPhotoDto photoDto);
    Task DeleteHotelPhotoAsync(int id);
    Task<HotelPhotoDto> GetHotelPhotoByIdAsync(int id);
    Task<IEnumerable<HotelPhotoDto>> GetAllHotelPhotosAsync();
}