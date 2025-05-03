using BookingSystem.Application.Booking;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<RoomPricing> _pricingRepository;

        public BookingService(
            IRepository<Domain.Entities.Booking> bookingRepository,
            IRepository<Room> roomRepository,
            IRepository<RoomPricing> pricingRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _pricingRepository = pricingRepository;
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking != null ? MapToDto(booking) : null;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Select(MapToDto);
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            var isAvailable = await CheckRoomAvailabilityAsync(
                bookingDto.RoomId,
                bookingDto.CheckInDate,
                bookingDto.CheckOutDate);

            if (!isAvailable)
                throw new InvalidOperationException("Room is not available for the selected dates");

            var booking = new Domain.Entities.Booking
            {
                RoomId = bookingDto.RoomId,
                UserId = bookingDto.UserId,
                CheckInDate = bookingDto.CheckInDate,
                CheckOutDate = bookingDto.CheckOutDate,
                GuestCount = bookingDto.GuestCount,
                TotalPrice = await CalculateTotalPrice(bookingDto),
                Status = (int)BookingStatus.Confirmed
            };

            await _bookingRepository.AddAsync(booking);
            return MapToDto(booking);
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(UpdateBookingDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(dto.Id) 
                ?? throw new KeyNotFoundException("Booking not found");

            if (booking.CheckInDate != dto.CheckInDate ||
                booking.CheckOutDate != dto.CheckOutDate)
            {
                booking.TotalPrice = await CalculateTotalPrice(dto);
            }

            booking.CheckInDate = dto.CheckInDate;
            booking.CheckOutDate = dto.CheckOutDate;
            booking.GuestCount = dto.GuestCount;
            booking.Status = dto.Status;

            await _bookingRepository.UpdateAsync(booking);
            return MapToDto(booking);
        }

        public async Task DeleteBookingAsync(int id)
        {
            await _bookingRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await _bookingRepository.GetAllAsync(b => b.UserId == userId);
            return bookings.Select(MapToDto);
        }

        public async Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null || !room.IsAvailable) return false;

            var conflictingBookings = await _bookingRepository.GetAllAsync(b =>
                b.RoomId == roomId &&
                b.CheckInDate < checkOutDate &&
                b.CheckOutDate > checkInDate);

            return !conflictingBookings.Any();
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found");

            booking.Status = (int)BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);
            return MapToDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _bookingRepository.GetAllAsync(b =>
                b.CheckInDate >= startDate &&
                b.CheckOutDate <= endDate);

            return bookings.Select(MapToDto);
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(int id, int statusCode)
        {
            if (!Enum.IsDefined(typeof(BookingStatus), statusCode))
                throw new ArgumentException("Invalid booking status code");

            var booking = await _bookingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found");

            booking.Status = statusCode;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);
            return MapToDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByRoomIdAsync(int roomId)
        {
            var bookings = await _bookingRepository.GetAllAsync(b => b.RoomId == roomId);
            return bookings.Select(MapToDto);
        }

        private async Task<decimal> CalculateTotalPrice(CreateBookingDto dto)
        {
            var pricing = await _pricingRepository.GetAllAsync(rp => 
                rp.RoomId == dto.RoomId &&
                rp.Date >= dto.CheckInDate &&
                rp.Date < dto.CheckOutDate);

            if (!pricing.Any())
                throw new InvalidOperationException("No pricing defined for selected dates");

            return pricing.Sum(p => p.Price);
        }

        private static BookingResponseDto MapToDto(Domain.Entities.Booking booking) => new()
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            UserId = booking.UserId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            GuestCount = booking.GuestCount,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        };
    }
}