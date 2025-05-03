using AutoFixture;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;
using Moq;
using System.Linq.Expressions;
using BookingSystem.Application.Booking;
using Xunit;

namespace BookingSystem.Application.Tests;

public class BookingServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRepository<Domain.Entities.Booking>> _bookingRepoMock;
    private readonly Mock<IRepository<Room>> _roomRepoMock;
    private readonly Mock<IRepository<RoomPricing>> _pricingRepoMock;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior(2));

        // Customize entities to break circular references
        _fixture.Customize<Room>(c => c
            .Without(r => r.RoomType)
            .Without(r => r.Bookings));

        _fixture.Customize<Domain.Entities.Booking>(c => c
            .Without(b => b.Room)
            .Without(b => b.User));

        _bookingRepoMock = new Mock<IRepository<Domain.Entities.Booking>>();
        _roomRepoMock = new Mock<IRepository<Room>>();
        _pricingRepoMock = new Mock<IRepository<RoomPricing>>();

        _service = new BookingService(
            _bookingRepoMock.Object,
            _roomRepoMock.Object,
            _pricingRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateBookingAsync_ValidRequest_ReturnsBooking()
    {
        // Arrange
        var roomId = 1;
        var dto = _fixture.Build<CreateBookingDto>()
            .With(x => x.RoomId, roomId)
            .Create();

        var room = _fixture.Build<Room>()
            .With(r => r.IsAvailable, true)
            .Create();

        var pricings = new List<RoomPricing>
        {
            new() { Date = dto.CheckInDate, Price = 100 },
            new() { Date = dto.CheckInDate.AddDays(1), Price = 100 }
        };

        _roomRepoMock.Setup(x => x.GetByIdAsync(roomId))
            .ReturnsAsync(room);
    
        _pricingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<RoomPricing, bool>>>()))
            .ReturnsAsync(pricings);

        // Fixed setup
        _bookingRepoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Booking>()))
            .Returns<Domain.Entities.Booking>(entity => Task.FromResult(entity));

        // Act
        var result = await _service.CreateBookingAsync(dto);

        // Assert
        Assert.Equal(200m, result.TotalPrice);
        _bookingRepoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Booking>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_RoomNotAvailable_ThrowsException()
    {
        // Arrange
        var dto = _fixture.Create<CreateBookingDto>();
        _roomRepoMock.Setup(x => x.GetByIdAsync(dto.RoomId))
            .ReturnsAsync((Room)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateBookingAsync(dto));
    }

    [Fact]
    public async Task UpdateBookingAsync_DatesChanged_RecalculatesPrice()
    {
        // Arrange
        var originalBooking = _fixture.Build<Domain.Entities.Booking>()
            .With(b => b.TotalPrice, 200m)
            .Create();

        var dto = _fixture.Build<UpdateBookingDto>()
            .With(x => x.CheckInDate, originalBooking.CheckInDate.AddDays(1))
            .With(x => x.CheckOutDate, originalBooking.CheckOutDate.AddDays(2))
            .Create();

        var pricings = new List<RoomPricing>
        {
            new() { Date = dto.CheckInDate, Price = 150 },
            new() { Date = dto.CheckInDate.AddDays(1), Price = 150 }
        };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(dto.Id))
            .ReturnsAsync(originalBooking);
        
        _pricingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<RoomPricing, bool>>>()))
            .ReturnsAsync(pricings);

        // Act
        var result = await _service.UpdateBookingAsync(dto);

        // Assert
        Assert.Equal(300m, result.TotalPrice);
    }

    [Fact]
    public async Task UpdateBookingAsync_NoDateChange_KeepsOriginalPrice()
    {
        // Arrange
        var originalBooking = _fixture.Build<Domain.Entities.Booking>()
            .With(b => b.TotalPrice, 200m)
            .Create();

        var dto = _fixture.Build<UpdateBookingDto>()
            .With(x => x.CheckInDate, originalBooking.CheckInDate)
            .With(x => x.CheckOutDate, originalBooking.CheckOutDate)
            .Create();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(dto.Id))
            .ReturnsAsync(originalBooking);

        // Act
        var result = await _service.UpdateBookingAsync(dto);

        // Assert
        Assert.Equal(200m, result.TotalPrice);
    }

    [Fact]
    public async Task GetBookingByIdAsync_Exists_ReturnsBooking()
    {
        // Arrange
        var booking = _fixture.Create<Domain.Entities.Booking>();
        _bookingRepoMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(booking);

        // Act
        var result = await _service.GetBookingByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_NotExists_ReturnsNull()
    {
        // Arrange
        _bookingRepoMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Domain.Entities.Booking)null);

        // Act
        var result = await _service.GetBookingByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllBookingsAsync_ReturnsAllBookings()
    {
        // Arrange
        var bookings = _fixture.CreateMany<Domain.Entities.Booking>(5);
        _bookingRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(bookings);

        // Act
        var result = await _service.GetAllBookingsAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task DeleteBookingAsync_ValidId_DeletesBooking()
    {
        // Arrange
        const int bookingId = 1;

        // Act
        await _service.DeleteBookingAsync(bookingId);

        // Assert
        _bookingRepoMock.Verify(x => x.DeleteAsync(bookingId), Times.Once);
    }

    [Fact]
    public async Task CheckRoomAvailabilityAsync_NoConflicts_ReturnsTrue()
    {
        // Arrange
        const int roomId = 1;
        var checkIn = DateTime.Today;
        var checkOut = DateTime.Today.AddDays(2);

        _roomRepoMock.Setup(x => x.GetByIdAsync(roomId))
            .ReturnsAsync(new Room { IsAvailable = true });
        
        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Domain.Entities.Booking, bool>>>()))
            .ReturnsAsync(new List<Domain.Entities.Booking>());

        // Act
        var result = await _service.CheckRoomAvailabilityAsync(roomId, checkIn, checkOut);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckRoomAvailabilityAsync_WithConflicts_ReturnsFalse()
    {
        // Arrange
        const int roomId = 1;
        var checkIn = DateTime.Today;
        var checkOut = DateTime.Today.AddDays(2);

        var conflictingBookings = new List<Domain.Entities.Booking>
        {
            _fixture.Build<Domain.Entities.Booking>()
                .With(b => b.RoomId, roomId)
                .With(b => b.CheckInDate, checkIn.AddDays(-1))
                .With(b => b.CheckOutDate, checkIn.AddDays(1))
                .Create()
        };

        _roomRepoMock.Setup(x => x.GetByIdAsync(roomId))
            .ReturnsAsync(new Room { IsAvailable = true });
        
        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Domain.Entities.Booking, bool>>>()))
            .ReturnsAsync(conflictingBookings);

        // Act
        var result = await _service.CheckRoomAvailabilityAsync(roomId, checkIn, checkOut);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CancelBookingAsync_ValidId_UpdatesStatus()
    {
        // Arrange
        var booking = _fixture.Build<Domain.Entities.Booking>()
            .With(b => b.Status, (int)BookingStatus.Confirmed)
            .Create();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(booking);

        // Act
        var result = await _service.CancelBookingAsync(1);

        // Assert
        Assert.Equal((int)BookingStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task GetBookingsByDateRangeAsync_ReturnsFilteredBookings()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(7);
        
        var bookings = new List<Domain.Entities.Booking>
        {
            _fixture.Build<Domain.Entities.Booking>()
                .With(b => b.CheckInDate, startDate.AddDays(1))
                .With(b => b.CheckOutDate, startDate.AddDays(3))
                .Create(),
            _fixture.Build<Domain.Entities.Booking>()
                .With(b => b.CheckInDate, startDate.AddDays(5))
                .With(b => b.CheckOutDate, startDate.AddDays(6))
                .Create()
        };

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Domain.Entities.Booking, bool>>>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _service.GetBookingsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateBookingStatusAsync_ValidStatus_UpdatesSuccessfully()
    {
        // Arrange
        var booking = _fixture.Create<Domain.Entities.Booking>();
        const int newStatus = (int)BookingStatus.Completed;

        _bookingRepoMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(booking);

        // Act
        var result = await _service.UpdateBookingStatusAsync(1, newStatus);

        // Assert
        Assert.Equal(newStatus, result.Status);
    }

    [Fact]
    public async Task UpdateBookingStatusAsync_InvalidStatus_ThrowsException()
    {
        // Arrange
        const int invalidStatusCode = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateBookingStatusAsync(1, invalidStatusCode));
    }

    [Fact]
    public async Task GetBookingsByRoomIdAsync_ReturnsRoomBookings()
    {
        // Arrange
        const int roomId = 1;
        var bookings = _fixture.Build<Domain.Entities.Booking>()
            .With(b => b.RoomId, roomId)
            .CreateMany(3);

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Domain.Entities.Booking, bool>>>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _service.GetBookingsByRoomIdAsync(roomId);

        // Assert
        Assert.Equal(3, result.Count());
    }
}