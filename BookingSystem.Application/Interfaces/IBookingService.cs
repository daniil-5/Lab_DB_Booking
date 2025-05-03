using BookingSystem.Application.Booking;

namespace BookingSystem.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync();
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto);
    Task<BookingResponseDto> UpdateBookingAsync(UpdateBookingDto bookingDto);
    Task DeleteBookingAsync(int id);
    Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId);
    Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime checkInDate, DateTime checkOutDate);
    Task<BookingResponseDto> CancelBookingAsync(int id);
    Task<IEnumerable<BookingResponseDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<BookingResponseDto> UpdateBookingStatusAsync(int id, int status);
    Task<IEnumerable<BookingResponseDto>> GetBookingsByRoomIdAsync(int roomId);
}