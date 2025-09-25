using System.Linq.Expressions;
using AutoFixture;
using BookingSystem.Application.DTOs.Hotel;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Moq;
using Xunit;

namespace BookingSystem.Tests
{
    public class HotelServiceTests
    {
        private readonly Mock<IHotelRepository> _hotelRepoMock;
        private readonly HotelService _service;
        private readonly Fixture _fixture;

        public HotelServiceTests()
        {
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _hotelRepoMock = new Mock<IHotelRepository>();
            _service = new HotelService(_hotelRepoMock.Object);
        }

        [Fact]
        public async Task CreateHotelAsync_ValidInput_CreatesAndReturnsHotel()
        {
            // Arrange
            var createDto = _fixture.Build<CreateHotelDto>()
                .With(x => x.Name, "Test Hotel")
                .With(x => x.Description, "Test Description")
                .With(x => x.Location, "Test Location")
                .With(x => x.Rating, 4.5m)
                .With(x => x.BasePrice, 150m)
                .With(x => x.Amenities, new List<string> { "WiFi", "Parking" })
                .Without(x => x.RoomTypes)
                .Without(x => x.Photos)
                .Create();

            Hotel capturedHotel = null;
            _hotelRepoMock.Setup(x => x.AddAsync(It.IsAny<Hotel>()))
                .Callback<Hotel>(h => capturedHotel = h)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateHotelAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Description, result.Description);
            Assert.Equal(createDto.Location, result.Location);
            Assert.Equal(createDto.Rating, result.Rating);
            Assert.Equal(createDto.BasePrice, result.BasePrice);
            Assert.Equal(createDto.Amenities.Count, result.Amenities.Count);

            Assert.NotNull(capturedHotel);
            Assert.Equal(createDto.Name, capturedHotel.Name);
            Assert.Equal(createDto.Description, capturedHotel.Description);
            Assert.Equal(createDto.Location, capturedHotel.Location);
            Assert.Equal(createDto.Rating, capturedHotel.Rating);
            Assert.Equal(createDto.BasePrice, capturedHotel.BasePrice);
            Assert.Equal(createDto.Amenities.Count, capturedHotel.Amenities.Count);
        }

        [Fact]
        public async Task UpdateHotelAsync_ValidInput_UpdatesAndReturnsHotel()
        {
            // Arrange
            var hotelId = 1;
            var updateDto = new UpdateHotelDto
            {
                Id = hotelId,
                Name = "Updated Hotel",
                Description = "Updated Description",
                Location = "Updated Location",
                Rating = 4.8m,
                BasePrice = 200m,
                Amenities = new List<string> { "WiFi", "Pool", "Gym" }
            };

            var existingHotel = new Hotel
            {
                Id = hotelId,
                Name = "Original Hotel",
                Description = "Original Description",
                Location = "Original Location",
                Rating = 4.0m,
                BasePrice = 150m,
                Amenities = new List<string> { "WiFi" }
            };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(existingHotel);

            _hotelRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Hotel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateHotelAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.Location, result.Location);
            Assert.Equal(updateDto.Rating, result.Rating);
            Assert.Equal(updateDto.BasePrice, result.BasePrice);
            Assert.Equal(updateDto.Amenities.Count, result.Amenities.Count);

            // Verify the hotel was updated
            _hotelRepoMock.Verify(x => x.UpdateAsync(It.Is<Hotel>(h => 
                h.Id == hotelId && 
                h.Name == updateDto.Name && 
                h.Description == updateDto.Description &&
                h.Location == updateDto.Location &&
                h.Rating == updateDto.Rating &&
                h.BasePrice == updateDto.BasePrice)), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateHotelAsync_HotelNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hotelId = 999;
            var updateDto = new UpdateHotelDto
            {
                Id = hotelId,
                Name = "Updated Hotel",
                Description = "Updated Description",
                Location = "Updated Location",
                Rating = 4.8m
            };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateHotelAsync(updateDto));

            Assert.Contains($"Hotel with ID {hotelId} not found", exception.Message);
            _hotelRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Hotel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteHotelAsync_ValidId_DeletesHotel()
        {
            // Arrange
            var hotelId = 1;
            var hotel = new Hotel { Id = hotelId };

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            _hotelRepoMock.Setup(x => x.DeleteAsync(hotelId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteHotelAsync(hotelId);

            // Assert
            _hotelRepoMock.Verify(x => x.DeleteAsync(hotelId), Times.Once);
        }

        [Fact]
        public async Task DeleteHotelAsync_HotelNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hotelId = 999;

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteHotelAsync(hotelId));

            Assert.Contains($"Hotel with ID {hotelId} not found", exception.Message);
            _hotelRepoMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetHotelByIdAsync_ValidId_ReturnsHotel()
        {
            // Arrange
            var hotelId = 1;
            var hotel = CreateHotelWithRelations(hotelId);

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId, It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>()))
                .ReturnsAsync(hotel);

            // Act
            var result = await _service.GetHotelByIdAsync(hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(hotel.Id, result.Id);
            Assert.Equal(hotel.Name, result.Name);
            Assert.Equal(hotel.Description, result.Description);
            Assert.Equal(hotel.Location, result.Location);
            Assert.Equal(hotel.Rating, result.Rating);
            Assert.Equal(hotel.BasePrice, result.BasePrice);
            Assert.Equal(hotel.RoomTypes.Count, result.RoomTypes.Count);
            Assert.Equal(hotel.Photos.Count, result.Photos.Count);
            Assert.Equal(hotel.Amenities.Count, result.Amenities.Count);
        }

        [Fact]
        public async Task GetHotelByIdAsync_HotelNotFound_ReturnsNull()
        {
            // Arrange
            var hotelId = 999;

            _hotelRepoMock.Setup(x => x.GetByIdAsync(hotelId, It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>()))
                .ReturnsAsync((Hotel)null);

            // Act
            var result = await _service.GetHotelByIdAsync(hotelId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllHotelsAsync_ReturnsAllHotels()
        {
            // Arrange
            var hotels = new List<Hotel>
            {
                CreateHotelWithRelations(1, "Hotel A"),
                CreateHotelWithRelations(2, "Hotel B")
            };

            _hotelRepoMock.Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Hotel, bool>>>(),
                    It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>()))
                .ReturnsAsync(hotels);

            // Act
            var results = await _service.GetAllHotelsAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(hotels.Count, results.Count());

            var resultsList = results.ToList();
            for (int i = 0; i < hotels.Count; i++)
            {
                Assert.Equal(hotels[i].Id, resultsList[i].Id);
                Assert.Equal(hotels[i].Name, resultsList[i].Name);
                Assert.Equal(hotels[i].Description, resultsList[i].Description);
                Assert.Equal(hotels[i].Location, resultsList[i].Location);
                Assert.Equal(hotels[i].Rating, resultsList[i].Rating);
                Assert.Equal(hotels[i].BasePrice, resultsList[i].BasePrice);
            }
        }

        [Fact]
        public async Task SearchHotelsAsync_ReturnsFilteredAndPaginatedHotels()
        {
            // Arrange
            var searchDto = new HotelSearchDto
            {
                Name = "Test",
                Location = "City",
                MinRating = 3.0m,
                MaxRating = 5.0m,
                MinPrice = 100m,
                MaxPrice = 300m,
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Rating",
                SortDescending = true
            };

            var hotels = new List<Hotel>
            {
                CreateHotelWithRelations(1, "Test Hotel 1", rating: 4.5m, basePrice: 150m),
                CreateHotelWithRelations(2, "Test Hotel 2", rating: 4.0m, basePrice: 200m)
            };

            int totalCount = 2;

            _hotelRepoMock.Setup(x => x.SearchHotelsAsync(
                    It.IsAny<Expression<Func<Hotel, bool>>>(),
                    It.IsAny<Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((hotels, totalCount));

            // Act
            var result = await _service.SearchHotelsAsync(searchDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(hotels.Count, result.Hotels.Count());
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(searchDto.PageNumber, result.PageNumber);
            Assert.Equal(searchDto.PageSize, result.PageSize);
            Assert.Equal((int)Math.Ceiling(totalCount / (double)searchDto.PageSize), result.TotalPages);
            Assert.False(result.HasPrevious); // First page
            Assert.False(result.HasNext); // Last page

            // Verify hotel mappings
            var resultHotels = result.Hotels.ToList();
            for (int i = 0; i < hotels.Count; i++)
            {
                Assert.Equal(hotels[i].Id, resultHotels[i].Id);
                Assert.Equal(hotels[i].Name, resultHotels[i].Name);
                Assert.Equal(hotels[i].Rating, resultHotels[i].Rating);
                Assert.Equal(hotels[i].BasePrice, resultHotels[i].BasePrice);
            }
        }

        // Helper method to create a hotel with relationships
        private Hotel CreateHotelWithRelations(
            int id,
            string name = "Test Hotel",
            string description = "Test Description",
            string location = "Test Location",
            decimal rating = 4.0m,
            decimal basePrice = 200m)
        {
            return new Hotel
            {
                Id = id,
                Name = name,
                Description = description,
                Location = location,
                Rating = rating,
                BasePrice = basePrice,
                Amenities = new List<string> { "WiFi", "Pool", "Parking" },
                RoomTypes = new List<RoomType>
                {
                    new RoomType { Id = id * 10 + 1, Name = "Standard", Capacity = 2 },
                    new RoomType { Id = id * 10 + 2, Name = "Deluxe", Capacity = 3 }
                },
                Photos = new List<HotelPhoto>
                {
                    new HotelPhoto { Id = id * 100 + 1, Url = $"http://example.com/hotel{id}_1.jpg", IsMain = true },
                    new HotelPhoto { Id = id * 100 + 2, Url = $"http://example.com/hotel{id}_2.jpg", IsMain = false }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };
        }
    }
}