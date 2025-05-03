using AutoFixture;
using BookingSystem.Application.DTOs.Room;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using System.Linq;
using Xunit;

namespace BookingSystem.Application.Tests;

public class RoomServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRepository<Room>> _repoMock;
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));
        _repoMock = new Mock<IRepository<Room>>();
        _service = new RoomService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateRoomAsync_ValidInput_CreatesRoom()
    {
        // Arrange
        var dto = _fixture.Create<CreateRoomDto>();
        var room = _fixture.Build<Room>()
            .With(r => r.RoomNumber, dto.RoomNumber)
            .Create();
        
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Room>()))
            .Returns(Task.FromResult(room));

        // Act
        var result = await _service.CreateRoomAsync(dto);

        // Assert
        Assert.Equal(dto.RoomNumber, result.RoomNumber);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_ValidInput_UpdatesRoom()
    {
        // Arrange
        var existingRoom = _fixture.Create<Room>();
        var dto = _fixture.Build<UpdateRoomDto>()
            .With(x => x.Id, existingRoom.Id)
            .Create();
        
        _repoMock.Setup(x => x.GetByIdAsync(dto.Id))
            .Returns(Task.FromResult(existingRoom));
        
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateRoomAsync(dto);

        // Assert
        Assert.Equal(dto.IsAvailable, result.IsAvailable);
        _repoMock.Verify(x => x.UpdateAsync(existingRoom), Times.Once);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Exists_ReturnsRoom()
    {
        // Arrange
        var room = _fixture.Create<Room>();
        _repoMock.Setup(x => x.GetByIdAsync(1))
            .Returns(Task.FromResult(room));

        // Act
        var result = await _service.GetRoomByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsAllRooms()
    {
        // Arrange
        var rooms = _fixture.CreateMany<Room>(5);
        _repoMock.Setup(x => x.GetAllAsync())
            .Returns(Task.FromResult(rooms.AsEnumerable()));

        // Act
        var result = await _service.GetAllRoomsAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task DeleteRoomAsync_ValidId_DeletesRoom()
    {
        // Arrange
        var roomId = 1;
        _repoMock.Setup(x => x.DeleteAsync(roomId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteRoomAsync(roomId);

        // Assert
        _repoMock.Verify(x => x.DeleteAsync(roomId), Times.Once);
    }
}