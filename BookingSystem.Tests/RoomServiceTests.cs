using AutoFixture;
using AutoFixture.Kernel;
using BookingSystem.Application.DTOs.Room;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookingSystem.Tests
{
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
            Room capturedRoom = null;

            _repoMock.Setup(x => x.AddAsync(It.IsAny<Room>()))
                .Callback<Room>(r => capturedRoom = r)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateRoomAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.RoomNumber, result.RoomNumber);
            Assert.Equal(dto.RoomTypeId, result.RoomTypeId);
            Assert.Equal(dto.IsAvailable, result.IsAvailable);

            Assert.NotNull(capturedRoom);
            Assert.Equal(dto.RoomNumber, capturedRoom.RoomNumber);
            Assert.Equal(dto.RoomTypeId, capturedRoom.RoomTypeId);
            Assert.Equal(dto.IsAvailable, capturedRoom.IsAvailable);
            
            _repoMock.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRoomAsync_ValidInput_UpdatesRoom()
        {
            // Arrange
            var roomId = 1;
            var dto = _fixture.Build<UpdateRoomDto>()
                .With(r => r.Id, roomId)
                .Create();

            var existingRoom = new Room
            {
                Id = roomId,
                RoomNumber = "Old-101",
                RoomTypeId = 2,
                IsAvailable = true
            };

            _repoMock.Setup(x => x.GetByIdAsync(roomId))
                .ReturnsAsync(existingRoom);

            _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateRoomAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.RoomNumber, result.RoomNumber);
            Assert.Equal(dto.RoomTypeId, result.RoomTypeId);
            Assert.Equal(dto.IsAvailable, result.IsAvailable);

            Assert.Equal(dto.RoomNumber, existingRoom.RoomNumber);
            Assert.Equal(dto.RoomTypeId, existingRoom.RoomTypeId);
            Assert.Equal(dto.IsAvailable, existingRoom.IsAvailable);
            
            _repoMock.Verify(x => x.GetByIdAsync(roomId), Times.Once);
            _repoMock.Verify(x => x.UpdateAsync(existingRoom), Times.Once);
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

        [Fact]
        public async Task GetRoomByIdAsync_ExistingId_ReturnsRoom()
        {
            // Arrange
            var roomId = 1;
            var room = new Room
            {
                Id = roomId,
                RoomNumber = "101",
                RoomTypeId = 1,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };

            _repoMock.Setup(x => x.GetByIdAsync(roomId))
                .ReturnsAsync(room);

            // Act
            var result = await _service.GetRoomByIdAsync(roomId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(room.Id, result.Id);
            Assert.Equal(room.RoomNumber, result.RoomNumber);
            Assert.Equal(room.RoomTypeId, result.RoomTypeId);
            Assert.Equal(room.IsAvailable, result.IsAvailable);
            Assert.Equal(room.CreatedAt, result.CreatedAt);
            Assert.Equal(room.UpdatedAt, result.UpdatedAt);
            
            _repoMock.Verify(x => x.GetByIdAsync(roomId), Times.Once);
        }

        [Fact]
        public async Task GetRoomByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var roomId = 999;

            _repoMock.Setup(x => x.GetByIdAsync(roomId))
                .ReturnsAsync((Room)null);

            // Act
            var result = await _service.GetRoomByIdAsync(roomId);

            // Assert
            Assert.Null(result);
            _repoMock.Verify(x => x.GetByIdAsync(roomId), Times.Once);
        }

        [Fact]
        public async Task GetAllRoomsAsync_ReturnsAllRooms()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = 1,
                    RoomNumber = "101",
                    RoomTypeId = 1,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Room
                {
                    Id = 2,
                    RoomNumber = "102",
                    RoomTypeId = 2,
                    IsAvailable = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _repoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(rooms);

            // Act
            var results = await _service.GetAllRoomsAsync();

            // Assert
            Assert.NotNull(results);
            var resultsList = results.ToList();
            Assert.Equal(rooms.Count, resultsList.Count);

            for (int i = 0; i < rooms.Count; i++)
            {
                Assert.Equal(rooms[i].Id, resultsList[i].Id);
                Assert.Equal(rooms[i].RoomNumber, resultsList[i].RoomNumber);
                Assert.Equal(rooms[i].RoomTypeId, resultsList[i].RoomTypeId);
                Assert.Equal(rooms[i].IsAvailable, resultsList[i].IsAvailable);
                Assert.Equal(rooms[i].CreatedAt, resultsList[i].CreatedAt);
                Assert.Equal(rooms[i].UpdatedAt, resultsList[i].UpdatedAt);
            }
            
            _repoMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}