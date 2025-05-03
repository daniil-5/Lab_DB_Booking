using AutoFixture;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using System.Linq;
using Xunit;

namespace BookingSystem.Application.Tests;

public class HotelServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRepository<Domain.Entities.Hotel>> _repoMock;
    private readonly HotelService _service;

    public HotelServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));
        _repoMock = new Mock<IRepository<Domain.Entities.Hotel>>();
    }

    [Fact]
    public async Task CreateHotelAsync_ValidInput_CreatesHotel()
    {
        // Arrange
        var dto = _fixture.Create<CreateHotelDto>();
        var hotel = _fixture.Build<Domain.Entities.Hotel>()
            .With(h => h.Name, dto.Name)
            .Create();
        
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Hotel>()))
            .Returns(Task.FromResult(hotel));

        // Act
        var result = await _service.CreateHotelAsync(dto);

        // Assert
        Assert.Equal(dto.Name, result.Name);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Hotel>()), Times.Once);
    }

    [Fact]
    public async Task UpdateHotelAsync_ValidInput_UpdatesHotel()
    {
        // Arrange
        var existingHotel = _fixture.Create<Domain.Entities.Hotel>();
        var dto = _fixture.Build<UpdateHotelDto>()
            .With(x => x.Id, existingHotel.Id)
            .Create();
        
        _repoMock.Setup(x => x.GetByIdAsync(dto.Id))
            .Returns(Task.FromResult(existingHotel));
        
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateHotelAsync(dto);

        // Assert
        Assert.Equal(dto.Location, result.Location);
        _repoMock.Verify(x => x.UpdateAsync(existingHotel), Times.Once);
    }

    [Fact]
    public async Task DeleteHotelAsync_ValidId_DeletesHotel()
    {
        // Arrange
        var hotelId = 1;
        _repoMock.Setup(x => x.DeleteAsync(hotelId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteHotelAsync(hotelId);

        // Assert
        _repoMock.Verify(x => x.DeleteAsync(hotelId), Times.Once);
    }

    [Fact]
    public async Task GetHotelByIdAsync_Exists_ReturnsHotel()
    {
        // Arrange
        var hotel = _fixture.Create<Domain.Entities.Hotel>();
        _repoMock.Setup(x => x.GetByIdAsync(1))
            .Returns(Task.FromResult(hotel));

        // Act
        var result = await _service.GetHotelByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hotel.Id, result.Id);
    }

    [Fact]
    public async Task GetAllHotelsAsync_ReturnsAllHotels()
    {
        // Arrange
        var hotels = _fixture.CreateMany<Domain.Entities.Hotel>(5);
        _repoMock.Setup(x => x.GetAllAsync())
            .Returns(Task.FromResult(hotels.AsEnumerable()));

        // Act
        var result = await _service.GetAllHotelsAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }
}