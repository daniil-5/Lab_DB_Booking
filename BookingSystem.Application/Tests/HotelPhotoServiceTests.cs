using AutoFixture;
using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using Xunit;

namespace BookingSystem.Application.Tests;

public class HotelPhotoServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRepository<HotelPhoto>> _repoMock;
    private readonly HotelPhotoService _service;

    public HotelPhotoServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));
        _repoMock = new Mock<IRepository<HotelPhoto>>();
        _service = new HotelPhotoService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateHotelPhotoAsync_ValidInput_ReturnsPhotoDto()
    {
        // Arrange
        var dto = _fixture.Create<CreateHotelPhotoDto>();
        var photo = _fixture.Build<HotelPhoto>()
            .With(h => h.IsMain, dto.IsMain)
            .Create();
        
        // Correct setup with explicit task creation
        _repoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
            .Returns(Task.FromResult(photo));

        // Act
        var result = await _service.CreateHotelPhotoAsync(dto);

        // Assert
        Assert.Equal(dto.IsMain, result.IsMain);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<HotelPhoto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateHotelPhotoAsync_ValidInput_UpdatesSuccessfully()
    {
        // Arrange
        var existingPhoto = _fixture.Create<HotelPhoto>();
        var dto = _fixture.Create<UpdateHotelPhotoDto>();
        
        // Proper async setup
        _repoMock.Setup(x => x.GetByIdAsync(dto.Id))
            .Returns(Task.FromResult(existingPhoto));
        
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<HotelPhoto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateHotelPhotoAsync(dto);

        // Assert
        Assert.Equal(dto.Url, result.Url);
        _repoMock.Verify(x => x.UpdateAsync(existingPhoto), Times.Once);
    }

    [Fact]
    public async Task DeleteHotelPhotoAsync_ValidId_DeletesSuccessfully()
    {
        // Arrange
        var photoId = 1;
        _repoMock.Setup(x => x.DeleteAsync(photoId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteHotelPhotoAsync(photoId);

        // Assert
        _repoMock.Verify(x => x.DeleteAsync(photoId), Times.Once);
    }

    [Fact]
    public async Task GetHotelPhotoByIdAsync_Exists_ReturnsPhoto()
    {
        // Arrange
        var photo = _fixture.Create<HotelPhoto>();
        _repoMock.Setup(x => x.GetByIdAsync(1))
            .Returns(Task.FromResult(photo));

        // Act
        var result = await _service.GetHotelPhotoByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(photo.Id, result.Id);
    }

    [Fact]
    public async Task GetAllHotelPhotosAsync_ReturnsAllPhotos()
    {
        // Arrange
        var photos = _fixture.CreateMany<HotelPhoto>(5);
        _repoMock.Setup(x => x.GetAllAsync())
            .Returns(Task.FromResult(photos.AsEnumerable()));

        // Act
        var result = await _service.GetAllHotelPhotosAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }
}