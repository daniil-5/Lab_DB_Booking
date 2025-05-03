using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class HotelPhotoService : IHotelPhotoService
{
    private readonly IRepository<HotelPhoto> _hotelPhotoRepository;

    public HotelPhotoService(IRepository<HotelPhoto> hotelPhotoRepository)
    {
        _hotelPhotoRepository = hotelPhotoRepository;
    }

    public async Task<HotelPhotoDto> CreateHotelPhotoAsync(CreateHotelPhotoDto photoDto)
    {
        var hotelPhoto = new HotelPhoto
        {
            HotelId = photoDto.HotelId,
            Url = photoDto.Url,
            PublicId = photoDto.PublicId,
            Description = photoDto.Description,
            IsMain = photoDto.IsMain
        };

        await _hotelPhotoRepository.AddAsync(hotelPhoto);
        return MapToDto(hotelPhoto);
    }

    public async Task<HotelPhotoDto> UpdateHotelPhotoAsync(UpdateHotelPhotoDto photoDto)
    {
        var existingPhoto = await _hotelPhotoRepository.GetByIdAsync(photoDto.Id);

        existingPhoto.HotelId = photoDto.HotelId;
        existingPhoto.Url = photoDto.Url;
        existingPhoto.PublicId = photoDto.PublicId;
        existingPhoto.Description = photoDto.Description;
        existingPhoto.IsMain = photoDto.IsMain;

        await _hotelPhotoRepository.UpdateAsync(existingPhoto);
        return MapToDto(existingPhoto);
    }

    public async Task DeleteHotelPhotoAsync(int id)
    {
        await _hotelPhotoRepository.DeleteAsync(id);
    }

    public async Task<HotelPhotoDto> GetHotelPhotoByIdAsync(int id)
    {
        var photo = await _hotelPhotoRepository.GetByIdAsync(id);
        return photo != null ? MapToDto(photo) : null;
    }

    public async Task<IEnumerable<HotelPhotoDto>> GetAllHotelPhotosAsync()
    {
        var photos = await _hotelPhotoRepository.GetAllAsync();
        return photos.Select(MapToDto).ToList();
    }

    private static HotelPhotoDto MapToDto(HotelPhoto photo)
    {
        return new HotelPhotoDto
        {
            Id = photo.Id,
            HotelId = photo.HotelId,
            Url = photo.Url,
            PublicId = photo.PublicId,
            Description = photo.Description,
            IsMain = photo.IsMain,
            CreatedAt = photo.CreatedAt,
            UpdatedAt = photo.UpdatedAt
        };
    }
}