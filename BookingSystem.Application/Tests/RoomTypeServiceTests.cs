// using AutoFixture;
// using BookingSystem.Application.RoomType;
// using BookingSystem.Application.Services;
// using BookingSystem.Domain.Entities;
// using BookingSystem.Domain.Interfaces;
// using Moq;
// using Xunit;
//
// namespace BookingSystem.Application.Tests;
//
// public class RoomTypeServiceTests
// {
//     private readonly Fixture _fixture;
//     private readonly Mock<IRepository<Domain.Entities.RoomType>> _roomTypeRepoMock;
//     private readonly Mock<IRepository<Domain.Entities.Hotel>> _hotelRepoMock;
//     private readonly RoomTypeService _service;
//
//     public RoomTypeServiceTests()
//     {
//         _fixture = new Fixture();
//         _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
//             .ForEach(b => _fixture.Behaviors.Remove(b));
//         _fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));
//         _roomTypeRepoMock = new Mock<IRepository<Domain.Entities.RoomType>>();
//         _hotelRepoMock = new Mock<IRepository<Domain.Entities.Hotel>>();
//         _service = new RoomTypeService(_roomTypeRepoMock.Object, _hotelRepoMock.Object);
//     }
//
//     #region CreateRoomTypeAsync Tests
//     [Fact]
//     public async Task CreateRoomTypeAsync_ValidInput_ReturnsRoomTypeDto()
//     {
//         // Arrange
//         var dto = _fixture.Build<CreateRoomTypeDto>()
//             .With(x => x.Name, "TestRoomType") // Explicit name
//             .Create();
//     
//         var hotel = _fixture.Create<Domain.Entities.Hotel>();
//         var expectedRoomType = _fixture.Build<Domain.Entities.RoomType>()
//             .With(rt => rt.HotelId, dto.HotelId)
//             .With(rt => rt.Name, dto.Name) // Match DTO name
//             .Create();
//
//         _hotelRepoMock.Setup(x => x.GetByIdAsync(dto.HotelId))
//             .ReturnsAsync(hotel);
//         _roomTypeRepoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.RoomType>()))
//             .Returns<Domain.Entities.RoomType>(entity => Task.FromResult(entity));
//
//         // Act
//         var result = await _service.CreateRoomTypeAsync(dto);
//
//         // Assert
//         Assert.Equal(dto.Name, result.Name); // Compare against DTO
//         _roomTypeRepoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RoomType>()), Times.Once);
//     }
//
//     [Fact]
//     public async Task CreateRoomTypeAsync_InvalidHotelId_ThrowsKeyNotFoundException()
//     {
//         // Arrange
//         var dto = _fixture.Create<CreateRoomTypeDto>();
//         _hotelRepoMock.Setup(x => x.GetByIdAsync(dto.HotelId))
//             .Returns(Task.FromResult((Domain.Entities.Hotel)null));
//
//         // Act & Assert
//         await Assert.ThrowsAsync<KeyNotFoundException>(
//             () => _service.CreateRoomTypeAsync(dto));
//     }
//     #endregion
//
//     #region UpdateRoomTypeAsync Tests
//     [Fact]
//     public async Task UpdateRoomTypeAsync_ValidInput_UpdatesRoomType()
//     {
//         // Arrange
//         var existingRoomType = _fixture.Create<Domain.Entities.RoomType>();
//         var dto = _fixture.Build<UpdateRoomTypeDto>()
//             .With(x => x.Id, existingRoomType.Id)
//             .With(x => x.HotelId, existingRoomType.HotelId)
//             .Create();
//         var hotel = _fixture.Create<Domain.Entities.Hotel>();
//
//         _roomTypeRepoMock.Setup(x => x.GetByIdAsync(dto.Id))
//             .Returns(Task.FromResult(existingRoomType));
//         _hotelRepoMock.Setup(x => x.GetByIdAsync(dto.HotelId))
//             .Returns(Task.FromResult(hotel));
//         _roomTypeRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.RoomType>()))
//             .Returns(Task.CompletedTask);
//
//         // Act
//         var result = await _service.UpdateRoomTypeAsync(dto);
//
//         // Assert
//         Assert.Equal(dto.Name, result.Name);
//         _roomTypeRepoMock.Verify(x => x.UpdateAsync(existingRoomType), Times.Once);
//     }
//
//     [Fact]
//     public async Task UpdateRoomTypeAsync_RoomTypeNotFound_ThrowsKeyNotFoundException()
//     {
//         // Arrange
//         var dto = _fixture.Create<UpdateRoomTypeDto>();
//         _roomTypeRepoMock.Setup(x => x.GetByIdAsync(dto.Id))
//             .Returns(Task.FromResult((Domain.Entities.RoomType)null));
//
//         // Act & Assert
//         await Assert.ThrowsAsync<KeyNotFoundException>(
//             () => _service.UpdateRoomTypeAsync(dto));
//     }
//     #endregion
//
//     #region DeleteRoomTypeAsync Tests
//     [Fact]
//     public async Task DeleteRoomTypeAsync_ValidId_DeletesRoomType()
//     {
//         // Arrange
//         var roomTypeId = 1;
//         var roomType = _fixture.Build<Domain.Entities.RoomType>()
//             .With(rt => rt.Rooms, new List<Room>())
//             .Create();
//
//         _roomTypeRepoMock.Setup(x => x.GetByIdAsync(roomTypeId))
//             .Returns(Task.FromResult(roomType));
//         _roomTypeRepoMock.Setup(x => x.DeleteAsync(roomTypeId))
//             .Returns(Task.CompletedTask);
//
//         // Act
//         await _service.DeleteRoomTypeAsync(roomTypeId);
//
//         // Assert
//         _roomTypeRepoMock.Verify(x => x.DeleteAsync(roomTypeId), Times.Once);
//     }
//
//     [Fact]
//     public async Task DeleteRoomTypeAsync_WithAssociatedRooms_ThrowsInvalidOperationException()
//     {
//         // Arrange
//         var roomType = _fixture.Build<Domain.Entities.RoomType>()
//             .With(rt => rt.Rooms, new List<Room> { new Room() })
//             .Create();
//
//         _roomTypeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
//             .Returns(Task.FromResult(roomType));
//
//         // Act & Assert
//         await Assert.ThrowsAsync<InvalidOperationException>(
//             () => _service.DeleteRoomTypeAsync(1));
//     }
//     #endregion
//
//     #region Get Methods Tests
//     [Fact]
//     public async Task GetRoomTypeByIdAsync_Exists_ReturnsRoomTypeDto()
//     {
//         // Arrange
//         var roomType = _fixture.Create<Domain.Entities.RoomType>();
//         _roomTypeRepoMock.Setup(x => x.GetByIdAsync(1))
//             .Returns(Task.FromResult(roomType));
//
//         // Act
//         var result = await _service.GetRoomTypeByIdAsync(1);
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.Equal(roomType.Id, result.Id);
//     }
//
//     [Fact]
//     public async Task GetAllRoomTypesAsync_ReturnsAllRoomTypes()
//     {
//         // Arrange
//         var roomTypes = _fixture.CreateMany<Domain.Entities.RoomType>(5);
//         _roomTypeRepoMock.Setup(x => x.GetAllAsync())
//             .Returns(Task.FromResult(roomTypes.AsEnumerable()));
//
//         // Act
//         var result = await _service.GetAllRoomTypesAsync();
//
//         // Assert
//         Assert.Equal(5, result.Count());
//     }
//     #endregion
//
//     #region Mapping Tests
//     [Fact]
//     public async Task CreateRoomTypeAsync_ProperMapping_ReturnsCorrectDto()
//     {
//         // Arrange
//         var dto = _fixture.Build<CreateRoomTypeDto>()
//             .With(x => x.Area, 50)
//             .With(x => x.Floor, 3)
//             .Create();
//
//         var hotel = _fixture.Create<Domain.Entities.Hotel>();
//         var roomType = _fixture.Build<Domain.Entities.RoomType>()
//             .With(rt => rt.HotelId, dto.HotelId)
//             .With(rt => rt.Area, dto.Area)
//             .With(rt => rt.Floor, dto.Floor)
//             .Create();
//
//         _hotelRepoMock.Setup(x => x.GetByIdAsync(dto.HotelId))
//             .ReturnsAsync(hotel);
//         _roomTypeRepoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.RoomType>()))
//             .Returns<Domain.Entities.RoomType>(entity => Task.FromResult(entity));
//         
//
//         // Act
//         var result = await _service.CreateRoomTypeAsync(dto);
//
//         // Assert
//         Assert.Equal(dto.Area, result.Area);
//         Assert.Equal(dto.Floor, result.Floor);
//     }
//     #endregion
// }