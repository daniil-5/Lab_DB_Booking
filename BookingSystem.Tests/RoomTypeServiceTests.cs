using AutoFixture;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using System.Linq.Expressions;
using BookingSystem.Application.DTOs.RoomType;
using Xunit;

namespace BookingSystem.Tests
{
    public class RoomTypeServiceTests
    {
        private readonly Mock<IRepository<RoomType>> _roomTypeRepoMock;
        private readonly Mock<IHotelRepository> _hotelRepoMock;
        private readonly RoomTypeService _service;
        private readonly Fixture _fixture;

        public RoomTypeServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _roomTypeRepoMock = new Mock<IRepository<RoomType>>();
            _hotelRepoMock = new Mock<IHotelRepository>();
            _service = new RoomTypeService(_roomTypeRepoMock.Object, _hotelRepoMock.Object);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_ValidInput_CreatesRoomType()
        {
            // Arrange
            var hotelId = 1;
            var createDto = new CreateRoomTypeDto
            {
                Name = "Deluxe Suite",
                Description = "Spacious suite with ocean view",
                Capacity = 3,
                BasePrice = 200.0m,
                Area = 50.0m,
                Floor = 5,
                HotelId = hotelId
            };

            var hotel = new Hotel { Id = hotelId, Name = "Test Hotel" };
            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            RoomType capturedRoomType = null;
            _roomTypeRepoMock.Setup(x => x.AddAsync(It.IsAny<RoomType>()))
                .Callback<RoomType>(rt => capturedRoomType = rt)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateRoomTypeAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Description, result.Description);
            Assert.Equal(createDto.Capacity, result.Capacity);
            Assert.Equal(createDto.BasePrice, result.BasePrice);
            Assert.Equal(createDto.Area, result.Area);
            Assert.Equal(createDto.Floor, result.Floor);
            Assert.Equal(createDto.HotelId, result.HotelId);

            Assert.NotNull(capturedRoomType);
            Assert.Equal(createDto.Name, capturedRoomType.Name);
            Assert.Equal(createDto.Description, capturedRoomType.Description);
            Assert.Equal(createDto.Capacity, capturedRoomType.Capacity);
            Assert.Equal(createDto.BasePrice, capturedRoomType.BasePrice);
            Assert.Equal(createDto.Area, capturedRoomType.Area);
            Assert.Equal(createDto.Floor, capturedRoomType.Floor);
            Assert.Equal(createDto.HotelId, capturedRoomType.HotelId);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_HotelNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hotelId = 999;
            var createDto = new CreateRoomTypeDto
            {
                Name = "Standard Room",
                Description = "Cozy room with city view",
                Capacity = 2,
                BasePrice = 150.0m,
                Area = 30.0m,
                Floor = 3,
                HotelId = hotelId
            };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.CreateRoomTypeAsync(createDto));
            
            Assert.Contains($"Hotel with ID {hotelId} not found", exception.Message);
            _roomTypeRepoMock.Verify(x => x.AddAsync(It.IsAny<RoomType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoomTypeAsync_ValidInput_UpdatesRoomType()
        {
            // Arrange
            var roomTypeId = 1;
            var hotelId = 1;
            var updateDto = new UpdateRoomTypeDto
            {
                Id = roomTypeId,
                Name = "Updated Suite",
                Description = "Updated description",
                Capacity = 4,
                BasePrice = 250.0m,
                Area = 60.0m,
                Floor = 6,
                HotelId = hotelId
            };

            var hotel = new Hotel { Id = hotelId, Name = "Test Hotel" };
            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            var existingRoomType = new RoomType
            {
                Id = roomTypeId,
                Name = "Old Suite",
                Description = "Old description",
                Capacity = 2,
                BasePrice = 200.0m,
                Area = 50.0m,
                Floor = 5,
                HotelId = hotelId
            };

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync(existingRoomType);

            _roomTypeRepoMock.Setup(x => x.UpdateAsync(It.IsAny<RoomType>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateRoomTypeAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Id, result.Id);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.Capacity, result.Capacity);
            Assert.Equal(updateDto.BasePrice, result.BasePrice);
            Assert.Equal(updateDto.Area, result.Area);
            Assert.Equal(updateDto.Floor, result.Floor);
            Assert.Equal(updateDto.HotelId, result.HotelId);

            // Verify the existing room type was updated
            Assert.Equal(updateDto.Name, existingRoomType.Name);
            Assert.Equal(updateDto.Description, existingRoomType.Description);
            Assert.Equal(updateDto.Capacity, existingRoomType.Capacity);
            Assert.Equal(updateDto.BasePrice, existingRoomType.BasePrice);
            Assert.Equal(updateDto.Area, existingRoomType.Area);
            Assert.Equal(updateDto.Floor, existingRoomType.Floor);
            Assert.Equal(updateDto.HotelId, existingRoomType.HotelId);

            _roomTypeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RoomType>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRoomTypeAsync_HotelNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var roomTypeId = 1;
            var hotelId = 999;
            var updateDto = new UpdateRoomTypeDto
            {
                Id = roomTypeId,
                Name = "Updated Suite",
                Description = "Updated description",
                Capacity = 4,
                BasePrice = 250.0m,
                Area = 60.0m,
                Floor = 6,
                HotelId = hotelId
            };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.UpdateRoomTypeAsync(updateDto));
            
            Assert.Contains($"Hotel with ID {hotelId} not found", exception.Message);
            _roomTypeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RoomType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRoomTypeAsync_RoomTypeNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var roomTypeId = 999;
            var hotelId = 1;
            var updateDto = new UpdateRoomTypeDto
            {
                Id = roomTypeId,
                Name = "Updated Suite",
                Description = "Updated description",
                Capacity = 4,
                BasePrice = 250.0m,
                Area = 60.0m,
                Floor = 6,
                HotelId = hotelId
            };

            var hotel = new Hotel { Id = hotelId, Name = "Test Hotel" };
            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync((RoomType)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.UpdateRoomTypeAsync(updateDto));
            
            Assert.Contains($"RoomType with ID {roomTypeId} not found", exception.Message);
            _roomTypeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RoomType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoomTypeAsync_ValidId_DeletesRoomType()
        {
            // Arrange
            var roomTypeId = 1;
            var roomType = new RoomType
            {
                Id = roomTypeId,
                Name = "Standard Room"
            };

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync(roomType);

            _roomTypeRepoMock.Setup(x => x.DeleteAsync(roomTypeId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteRoomTypeAsync(roomTypeId);

            // Assert
            _roomTypeRepoMock.Verify(x => x.DeleteAsync(roomTypeId), Times.Once);
        }

        [Fact]
        public async Task DeleteRoomTypeAsync_RoomTypeNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var roomTypeId = 999;

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync((RoomType)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.DeleteRoomTypeAsync(roomTypeId));
            
            Assert.Contains($"RoomType with ID {roomTypeId} not found", exception.Message);
            _roomTypeRepoMock.Verify(x => x.DeleteAsync(roomTypeId), Times.Never);
        }


        [Fact]
        public async Task GetRoomTypeByIdAsync_ExistingId_ReturnsRoomType()
        {
            // Arrange
            var roomTypeId = 1;
            var roomType = new RoomType
            {
                Id = roomTypeId,
                Name = "Deluxe Suite",
                Description = "Spacious suite with ocean view",
                Capacity = 3,
                BasePrice = 200.0m,
                Area = 50.0m,
                Floor = 5,
                HotelId = 1
            };

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync(roomType);

            // Act
            var result = await _service.GetRoomTypeByIdAsync(roomTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roomType.Id, result.Id);
            Assert.Equal(roomType.Name, result.Name);
            Assert.Equal(roomType.Description, result.Description);
            Assert.Equal(roomType.Capacity, result.Capacity);
            Assert.Equal(roomType.BasePrice, result.BasePrice);
            Assert.Equal(roomType.Area, result.Area);
            Assert.Equal(roomType.Floor, result.Floor);
            Assert.Equal(roomType.HotelId, result.HotelId);
        }

        [Fact]
        public async Task GetRoomTypeByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var roomTypeId = 999;

            _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
                .ReturnsAsync((RoomType)null);

            // Act
            var result = await _service.GetRoomTypeByIdAsync(roomTypeId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRoomTypesAsync_ReturnsAllRoomTypes()
        {
            // Arrange
            var roomTypes = new List<RoomType>
            {
                new RoomType
                {
                    Id = 1,
                    Name = "Standard Room",
                    Description = "Cozy room with city view",
                    Capacity = 2,
                    BasePrice = 150.0m,
                    Area = 30.0m,
                    Floor = 3,
                    HotelId = 1
                },
                new RoomType
                {
                    Id = 2,
                    Name = "Deluxe Suite",
                    Description = "Spacious suite with ocean view",
                    Capacity = 3,
                    BasePrice = 200.0m,
                    Area = 50.0m,
                    Floor = 5,
                    HotelId = 1
                }
            };

            _roomTypeRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(roomTypes);

            // Act
            var results = await _service.GetAllRoomTypesAsync();

            // Assert
            Assert.NotNull(results);
            var resultsList = results.ToList();
            Assert.Equal(roomTypes.Count, resultsList.Count);

            for (int i = 0; i < roomTypes.Count; i++)
            {
                Assert.Equal(roomTypes[i].Id, resultsList[i].Id);
                Assert.Equal(roomTypes[i].Name, resultsList[i].Name);
                Assert.Equal(roomTypes[i].Description, resultsList[i].Description);
                Assert.Equal(roomTypes[i].Capacity, resultsList[i].Capacity);
                Assert.Equal(roomTypes[i].BasePrice, resultsList[i].BasePrice);
                Assert.Equal(roomTypes[i].Area, resultsList[i].Area);
                Assert.Equal(roomTypes[i].Floor, resultsList[i].Floor);
                Assert.Equal(roomTypes[i].HotelId, resultsList[i].HotelId);
            }
        }

        [Fact]
        public async Task GetRoomTypesByHotelIdAsync_ValidHotelId_ReturnsRoomTypes()
        {
            // Arrange
            var hotelId = 1;
            var hotel = new Hotel { Id = hotelId, Name = "Test Hotel" };
            
            var roomTypes = new List<RoomType>
            {
                new RoomType
                {
                    Id = 1,
                    Name = "Standard Room",
                    Description = "Cozy room with city view",
                    Capacity = 2,
                    BasePrice = 150.0m,
                    Area = 30.0m,
                    Floor = 3,
                    HotelId = hotelId
                },
                new RoomType
                {
                    Id = 2,
                    Name = "Deluxe Suite",
                    Description = "Spacious suite with ocean view",
                    Capacity = 3,
                    BasePrice = 200.0m,
                    Area = 50.0m,
                    Floor = 5,
                    HotelId = hotelId
                }
            };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            _roomTypeRepoMock.Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<RoomType, bool>>>(),
                    It.IsAny<Func<IQueryable<RoomType>, IQueryable<RoomType>>>()))
                .ReturnsAsync(roomTypes);

            // Act
            var results = await _service.GetRoomTypesByHotelIdAsync(hotelId);

            // Assert
            Assert.NotNull(results);
            var resultsList = results.ToList();
            Assert.Equal(roomTypes.Count, resultsList.Count);

            for (int i = 0; i < roomTypes.Count; i++)
            {
                Assert.Equal(roomTypes[i].Id, resultsList[i].Id);
                Assert.Equal(roomTypes[i].Name, resultsList[i].Name);
                Assert.Equal(roomTypes[i].Description, resultsList[i].Description);
                Assert.Equal(roomTypes[i].Capacity, resultsList[i].Capacity);
                Assert.Equal(roomTypes[i].BasePrice, resultsList[i].BasePrice);
                Assert.Equal(roomTypes[i].Area, resultsList[i].Area);
                Assert.Equal(roomTypes[i].Floor, resultsList[i].Floor);
                Assert.Equal(roomTypes[i].HotelId, resultsList[i].HotelId);
            }
        }

        [Fact]
        public async Task GetRoomTypesByHotelIdAsync_HotelNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hotelId = 999;

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.GetRoomTypesByHotelIdAsync(hotelId));
            
            Assert.Contains($"Hotel with ID {hotelId} not found", exception.Message);
        }
    }
}