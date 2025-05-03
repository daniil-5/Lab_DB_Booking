using System.Diagnostics;
using BookingSystem.Application.Booking;
using BookingSystem.Application.DTOs.Room;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.RoomType;
using BookingSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookingSystem.API.Controllers;

public static class ControllerTests
{
    private static readonly DateTime CurrentTime = DateTime.Parse("2025-04-14 10:34:18");

    public static async Task RunAllTests()
    {
        Console.WriteLine($"=== Starting controller tests at {CurrentTime} ===");
        
        await TestRoomTypesController();
        await TestRoomsController();
        await TestHotelsController();
        await TestBookingsController();
        
        Console.WriteLine($"=== All controller tests completed successfully! ===");
    }

    #region RoomType Tests
    private static async Task TestRoomTypesController()
    {
        Console.WriteLine("Testing RoomTypesController...");
        
        var mockService = new Mock<IRoomTypeService>();
        
        var testRoomTypes = new List<RoomTypeDto>
        {
            new RoomTypeDto { Id = 1, Name = "Standard", BedCount = 1, Area = 20, HotelId = 1 },
            new RoomTypeDto { Id = 2, Name = "Deluxe", BedCount = 2, Area = 30, HotelId = 1 }
        };
        
        mockService.Setup(s => s.GetAllRoomTypesAsync())
            .ReturnsAsync(testRoomTypes);
        
        mockService.Setup(s => s.GetRoomTypeByIdAsync(1))
            .ReturnsAsync(testRoomTypes[0]);
        
        mockService.Setup(s => s.GetRoomTypeByIdAsync(999))
            .ReturnsAsync((RoomTypeDto)null);
            
        mockService.Setup(s => s.CreateRoomTypeAsync(It.IsAny<CreateRoomTypeDto>()))
            .ReturnsAsync(new RoomTypeDto { Id = 3, Name = "New Type", BedCount = 1, Area = 25, HotelId = 1 });
            
        mockService.Setup(s => s.UpdateRoomTypeAsync(It.IsAny<UpdateRoomTypeDto>()))
            .ReturnsAsync(new RoomTypeDto { Id = 1, Name = "Updated Standard", BedCount = 1, Area = 25, HotelId = 1 });
            
        mockService.Setup(s => s.DeleteRoomTypeAsync(1))
            .Returns(Task.CompletedTask);
            
        mockService.Setup(s => s.DeleteRoomTypeAsync(2))
            .ThrowsAsync(new InvalidOperationException("Cannot delete room type that is in use by rooms."));

        // Create controller with mock service
        var controller = new RoomTypesController(mockService.Object);
        
        // Test GetAllRoomTypes
        var getAllResult = await controller.GetAllRoomTypes();
        var getAllOkResult = AssertType<OkObjectResult>(getAllResult.Result, "GetAllRoomTypes should return OkObjectResult");
        var returnedRoomTypes = AssertType<IEnumerable<RoomTypeDto>>(getAllOkResult.Value, "Return value should be IEnumerable<RoomTypeDto>");
        AssertEqual(2, returnedRoomTypes.Count(), "Should return 2 room types");
        
        // Test GetRoomType - Found
        var getByIdResult = await controller.GetRoomType(1);
        var getByIdOkResult = AssertType<OkObjectResult>(getByIdResult.Result, "GetRoomType(1) should return OkObjectResult");
        var returnedRoomType = AssertType<RoomTypeDto>(getByIdOkResult.Value, "Return value should be RoomTypeDto");
        AssertEqual(1, returnedRoomType.Id, "Room type ID should be 1");
        AssertEqual("Standard", returnedRoomType.Name, "Room type name should be 'Standard'");
        
        // Test GetRoomType - Not Found
        var getByIdNotFoundResult = await controller.GetRoomType(999);
        AssertType<NotFoundObjectResult>(getByIdNotFoundResult.Result, "GetRoomType(999) should return NotFoundObjectResult");
        
        // Test CreateRoomType
        var createDto = new CreateRoomTypeDto { Name = "New Type", BedCount = 1, Area = 25, HotelId = 1 };
        var createResult = await controller.CreateRoomType(createDto);
        var createdAtActionResult = AssertType<CreatedAtActionResult>(createResult.Result, "CreateRoomType should return CreatedAtActionResult");
        AssertEqual("GetRoomType", createdAtActionResult.ActionName, "Action name should be GetRoomType");
        AssertEqual(3, createdAtActionResult.RouteValues["id"], "New room type ID should be 3");
        
        // Test UpdateRoomType
        var updateDto = new UpdateRoomTypeDto { Id = 1, Name = "Updated Standard", BedCount = 1, Area = 25, HotelId = 1 };
        var updateResult = await controller.UpdateRoomType(1, updateDto);
        var okResult = AssertType<OkObjectResult>(updateResult, "UpdateRoomType should return OkObjectResult");
        var updatedRoomType = AssertType<RoomTypeDto>(okResult.Value, "Return value should be RoomTypeDto");
        AssertEqual("Updated Standard", updatedRoomType.Name, "Updated name should be 'Updated Standard'");
        
        // Test DeleteRoomType - Success
        var deleteResult = await controller.DeleteRoomType(1);
        AssertType<NoContentResult>(deleteResult, "DeleteRoomType(1) should return NoContentResult");
        
        // Test DeleteRoomType - Conflict
        var deleteConflictResult = await controller.DeleteRoomType(2);
        AssertType<ConflictObjectResult>(deleteConflictResult, "DeleteRoomType(2) should return ConflictObjectResult");
        
        Console.WriteLine("✅ RoomTypesController tests passed!");
    }
    #endregion

    #region Room Tests
    private static async Task TestRoomsController()
    {
        Console.WriteLine("Testing RoomsController...");
        
        // Setup mock service
        var mockService = new Mock<IRoomService>();
        
        // Setup test data
        var testRooms = new List<RoomDto>
        {
            new RoomDto { Id = 1, RoomNumber = "101", RoomTypeId = 1, IsAvailable = true },
            new RoomDto { Id = 2, RoomNumber = "102", RoomTypeId = 1, IsAvailable = false }
        };
        
        // Configure mock service behavior
        mockService.Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(testRooms);
            
        mockService.Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(testRooms[0]);
            
        mockService.Setup(s => s.GetRoomByIdAsync(999))
            .ReturnsAsync((RoomDto)null);
            
        mockService.Setup(s => s.CreateRoomAsync(It.IsAny<CreateRoomDto>()))
            .ReturnsAsync(new RoomDto { Id = 3, RoomNumber = "103", RoomTypeId = 1, IsAvailable = true });
            
        mockService.Setup(s => s.UpdateRoomAsync(It.IsAny<UpdateRoomDto>()))
            .ReturnsAsync(new RoomDto { Id = 1, RoomNumber = "101-Updated", RoomTypeId = 1, IsAvailable = false });
            
        mockService.Setup(s => s.DeleteRoomAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        
        // Create controller with mock service
        var controller = new RoomsController(mockService.Object);
        
        // Test GetAllRooms
        var getAllResult = await controller.GetAllRooms();
        var getAllOkResult = AssertType<OkObjectResult>(getAllResult.Result, "GetAllRooms should return OkObjectResult");
        var returnedRooms = AssertType<IEnumerable<RoomDto>>(getAllOkResult.Value, "Return value should be IEnumerable<RoomDto>");
        AssertEqual(2, returnedRooms.Count(), "Should return 2 rooms");
        
        // Test GetRoom - Found
        var getByIdResult = await controller.GetRoom(1);
        var getByIdOkResult = AssertType<OkObjectResult>(getByIdResult.Result, "GetRoom(1) should return OkObjectResult");
        var returnedRoom = AssertType<RoomDto>(getByIdOkResult.Value, "Return value should be RoomDto");
        AssertEqual(1, returnedRoom.Id, "Room ID should be 1");
        AssertEqual("101", returnedRoom.RoomNumber, "Room number should be '101'");
        
        // Test GetRoom - Not Found
        var getByIdNotFoundResult = await controller.GetRoom(999);
        AssertType<NotFoundResult>(getByIdNotFoundResult.Result, "GetRoom(999) should return NotFoundResult");
        
        // Test CreateRoom
        var createDto = new CreateRoomDto { RoomNumber = "103", RoomTypeId = 1, IsAvailable = true };
        var createResult = await controller.CreateRoom(createDto);
        var createdAtActionResult = AssertType<CreatedAtActionResult>(createResult.Result, "CreateRoom should return CreatedAtActionResult");
        AssertEqual("GetRoom", createdAtActionResult.ActionName, "Action name should be GetRoom");
        AssertEqual(3, createdAtActionResult.RouteValues["id"], "New room ID should be 3");
        
        // Test UpdateRoom
        var updateDto = new UpdateRoomDto { Id = 1, RoomNumber = "101-Updated", RoomTypeId = 1, IsAvailable = false };
        var updateResult = await controller.UpdateRoom(1, updateDto);
        AssertType<NoContentResult>(updateResult, "UpdateRoom should return NoContentResult");
        
        // Test UpdateRoom - ID mismatch
        var updateMismatchResult = await controller.UpdateRoom(2, updateDto);
        AssertType<BadRequestObjectResult>(updateMismatchResult, "UpdateRoom with ID mismatch should return BadRequestObjectResult");
        
        // Test DeleteRoom
        var deleteResult = await controller.DeleteRoom(1);
        AssertType<NoContentResult>(deleteResult, "DeleteRoom should return NoContentResult");
        
        Console.WriteLine("✅ RoomsController tests passed!");
    }
    #endregion

    #region Hotel Tests
    private static async Task TestHotelsController()
    {
        Console.WriteLine("Testing HotelsController...");
        
        // Setup mock service
        var mockService = new Mock<IHotelService>();
        
        // Setup test data
        var testHotels = new List<HotelDto>
        {
            new HotelDto { Id = 1, Name = "Grand Hotel", Location = "Downtown", Rating = 4.5m },
            new HotelDto { Id = 2, Name = "Beach Resort", Location = "Coastline", Rating = 4.8m }
        };
        
        // Configure mock service behavior
        mockService.Setup(s => s.GetAllHotelsAsync())
            .ReturnsAsync(testHotels);
            
        mockService.Setup(s => s.GetHotelByIdAsync(1))
            .ReturnsAsync(testHotels[0]);
            
        mockService.Setup(s => s.GetHotelByIdAsync(999))
            .ReturnsAsync((HotelDto)null);
            
        mockService.Setup(s => s.CreateHotelAsync(It.IsAny<CreateHotelDto>()))
            .ReturnsAsync(new HotelDto { Id = 3, Name = "Mountain Lodge", Location = "Alpine", Rating = 4.2m });
            
        mockService.Setup(s => s.DeleteHotelAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        
        // Create controller with mock service
        var controller = new HotelsController(mockService.Object);
        
        // Test GetHotels
        var getHotelsResult = await controller.GetHotels();
        var getHotelsOkResult = AssertType<OkObjectResult>(getHotelsResult.Result, "GetHotels should return OkObjectResult");
        var returnedHotels = AssertType<IEnumerable<HotelDto>>(getHotelsOkResult.Value, "Return value should be IEnumerable<HotelDto>");
        AssertEqual(2, returnedHotels.Count(), "Should return 2 hotels");
        
        // Test GetHotel - Found
        var getHotelResult = await controller.GetHotel(1);
        var getHotelOkResult = AssertType<OkObjectResult>(getHotelResult.Result, "GetHotel(1) should return OkObjectResult");
        var returnedHotel = AssertType<HotelDto>(getHotelOkResult.Value, "Return value should be HotelDto");
        AssertEqual(1, returnedHotel.Id, "Hotel ID should be 1");
        AssertEqual("Grand Hotel", returnedHotel.Name, "Hotel name should be 'Grand Hotel'");
        
        // Test GetHotel - Not Found
        var getHotelNotFoundResult = await controller.GetHotel(999);
        AssertType<NotFoundResult>(getHotelNotFoundResult.Result, "GetHotel(999) should return NotFoundResult");
        
        // Test CreateHotel
        var createHotelDto = new CreateHotelDto { Name = "Mountain Lodge", Location = "Alpine", Rating = 4.2m };
        var createHotelResult = await controller.CreateHotel(createHotelDto);
        var createdHotelAtActionResult = AssertType<CreatedAtActionResult>(createHotelResult.Result, "CreateHotel should return CreatedAtActionResult");
        AssertEqual("GetHotel", createdHotelAtActionResult.ActionName, "Action name should be GetHotel");
        AssertEqual(3, createdHotelAtActionResult.RouteValues["id"], "New hotel ID should be 3");
        
        // Test UpdateHotel
        var updateHotelDto = new UpdateHotelDto { Id = 1, Name = "Updated Grand Hotel", Location = "City Center", Rating = 4.6m };
        var updateHotelResult = await controller.UpdateHotel(1, updateHotelDto);
        AssertType<NoContentResult>(updateHotelResult, "UpdateHotel should return NoContentResult");
        
        // Test UpdateHotel - ID mismatch
        var updateHotelMismatchResult = await controller.UpdateHotel(2, updateHotelDto);
        AssertType<BadRequestObjectResult>(updateHotelMismatchResult, "UpdateHotel with ID mismatch should return BadRequestObjectResult");
        
        // Test DeleteHotel
        var deleteHotelResult = await controller.DeleteHotel(1);
        AssertType<NoContentResult>(deleteHotelResult, "DeleteHotel should return NoContentResult");
        
        Console.WriteLine("✅ HotelsController tests passed!");
    }
    #endregion

    #region Booking Tests
    private static async Task TestBookingsController()
    {
        Console.WriteLine("Testing BookingsController...");
        
        // Setup mock service
        var mockService = new Mock<IBookingService>();
        
        // Setup test data
        var testBookings = new List<BookingResponseDto>
        {
            new BookingResponseDto { Id = 1, RoomId = 1, UserId = 1, CheckInDate = DateTime.Now.AddDays(1), CheckOutDate = DateTime.Now.AddDays(3) },
            new BookingResponseDto { Id = 2, RoomId = 2, UserId = 2, CheckInDate = DateTime.Now.AddDays(5), CheckOutDate = DateTime.Now.AddDays(7) }
        };
        
        // Configure mock service behavior
        mockService.Setup(s => s.GetAllBookingsAsync())
            .ReturnsAsync(testBookings);
            
        mockService.Setup(s => s.GetBookingByIdAsync(1))
            .ReturnsAsync(testBookings[0]);
            
        mockService.Setup(s => s.GetBookingByIdAsync(999))
            .ReturnsAsync((BookingResponseDto)null);
            
        mockService.Setup(s => s.CreateBookingAsync(It.IsAny<CreateBookingDto>()))
            .ReturnsAsync(new BookingResponseDto { Id = 3, RoomId = 3, UserId = 1, CheckInDate = DateTime.Now.AddDays(10), CheckOutDate = DateTime.Now.AddDays(12) });
            
        mockService.Setup(s => s.CreateBookingAsync(It.Is<CreateBookingDto>(b => b.RoomId == 999)))
            .ThrowsAsync(new Exception("Room not available"));
            
        mockService.Setup(s => s.DeleteBookingAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        
        // Create controller with mock service
        var controller = new BookingsController(mockService.Object);
        
        // Test GetBookings
        var getBookingsResult = await controller.GetBookings();
        var getBookingsOkResult = AssertType<OkObjectResult>(getBookingsResult.Result, "GetBookings should return OkObjectResult");
        var returnedBookings = AssertType<IEnumerable<BookingResponseDto>>(getBookingsOkResult.Value, "Return value should be IEnumerable<BookingResponseDto>");
        AssertEqual(2, returnedBookings.Count(), "Should return 2 bookings");
        
        // Test GetBooking - Found
        var getBookingResult = await controller.GetBooking(1);
        var getBookingOkResult = AssertType<OkObjectResult>(getBookingResult.Result, "GetBooking(1) should return OkObjectResult");
        var returnedBooking = AssertType<BookingResponseDto>(getBookingOkResult.Value, "Return value should be BookingResponseDto");
        AssertEqual(1, returnedBooking.Id, "Booking ID should be 1");
        
        // Test GetBooking - Not Found
        var getBookingNotFoundResult = await controller.GetBooking(999);
        AssertType<NotFoundResult>(getBookingNotFoundResult.Result, "GetBooking(999) should return NotFoundResult");
        
        // Test CreateBooking - Success
        var createBookingDto = new CreateBookingDto { RoomId = 3, UserId = 1, CheckInDate = DateTime.Now.AddDays(10), CheckOutDate = DateTime.Now.AddDays(12) };
        var createBookingResult = await controller.CreateBooking(createBookingDto);
        var createdBookingAtActionResult = AssertType<CreatedAtActionResult>(createBookingResult.Result, "CreateBooking should return CreatedAtActionResult");
        AssertEqual("GetBooking", createdBookingAtActionResult.ActionName, "Action name should be GetBooking");
        AssertEqual(3, createdBookingAtActionResult.RouteValues["id"], "New booking ID should be 3");
        
        // Test CreateBooking - Error
        var invalidCreateBookingDto = new CreateBookingDto { RoomId = 999, UserId = 1, CheckInDate = DateTime.Now.AddDays(1), CheckOutDate = DateTime.Now.AddDays(3) };
        var invalidCreateBookingResult = await controller.CreateBooking(invalidCreateBookingDto);
        AssertType<BadRequestObjectResult>(invalidCreateBookingResult.Result, "CreateBooking with invalid data should return BadRequestObjectResult");
        
        // Test UpdateBooking
        var updateBookingDto = new UpdateBookingDto { Id = 1, RoomId = 1, UserId = 1, CheckInDate = DateTime.Now.AddDays(2), CheckOutDate = DateTime.Now.AddDays(4) };
        var updateBookingResult = await controller.UpdateBooking(1, updateBookingDto);
        AssertType<NoContentResult>(updateBookingResult, "UpdateBooking should return NoContentResult");
        
        // Test UpdateBooking - ID mismatch
        var updateBookingMismatchResult = await controller.UpdateBooking(2, updateBookingDto);
        AssertType<BadRequestObjectResult>(updateBookingMismatchResult, "UpdateBooking with ID mismatch should return BadRequestObjectResult");
        
        // Test DeleteBooking
        var deleteBookingResult = await controller.DeleteBooking(1);
        AssertType<NoContentResult>(deleteBookingResult, "DeleteBooking should return NoContentResult");
        
        Console.WriteLine("✅ BookingsController tests passed!");
    }
    #endregion

    #region Helper Methods
    private static T AssertType<T>(object obj, string message = null)
    {
        if (obj is T result)
        {
            return result;
        }
        
        var actualType = obj?.GetType().Name ?? "null";
        var errorMessage = message ?? $"Expected type {typeof(T).Name}, but got {actualType}";
        Console.WriteLine($"❌ ASSERTION FAILED: {errorMessage}");
        Debug.Fail(errorMessage);
        throw new InvalidOperationException(errorMessage);
    }
    
    private static void AssertEqual<T>(T expected, T actual, string message = null)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            return;
        }
        
        var errorMessage = message ?? $"Expected: {expected}, Actual: {actual}";
        Console.WriteLine($"❌ ASSERTION FAILED: {errorMessage}");
        Debug.Fail(errorMessage);
        throw new InvalidOperationException(errorMessage);
    }
    #endregion
}