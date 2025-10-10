using BookingSystem.Application.DTOs.Booking;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;


namespace BookingSystem.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
        private readonly IRepository<Domain.Entities.RoomType> _roomTypeRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRepository<RoomPricing> _pricingRepository;
        private readonly IUserActionAuditService _auditService;

        public BookingService(
            IRepository<Domain.Entities.Booking> bookingRepository,
            IRepository<Domain.Entities.RoomType> roomTypeRepository,
            IHotelRepository hotelRepository,
            IRepository<RoomPricing> pricingRepository,
            IUserActionAuditService auditService)
        {
            _bookingRepository = bookingRepository;
            _roomTypeRepository = roomTypeRepository;
            _hotelRepository = hotelRepository;
            _pricingRepository = pricingRepository;
            _auditService = auditService;
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
            // Validate dates
            if (bookingDto.CheckInDate >= bookingDto.CheckOutDate)
            {
                throw new ArgumentException("Check-out date must be after check-in date");
            }

            // Check if hotel exists
            var hotel = await _hotelRepository.GetByIdAsync(bookingDto.HotelId);
            if (hotel == null)
            {
                throw new KeyNotFoundException($"Hotel with ID {bookingDto.HotelId} not found");
            }

            // Check if room type exists and belongs to the hotel
            var roomType = await _roomTypeRepository.GetByIdAsync(bookingDto.RoomTypeId);
            if (roomType == null || roomType.HotelId != bookingDto.HotelId)
            {
                throw new KeyNotFoundException($"Room type with ID {bookingDto.RoomTypeId} not found in hotel {bookingDto.HotelId}");
            }

            // Check if the number of guests is within the room type capacity
            if (bookingDto.GuestCount > roomType.Capacity)
            {
                throw new InvalidOperationException($"This room type can only accommodate {roomType.Capacity} guests");
            }

            // Check availability of the room type for the requested dates
            var isAvailable = await CheckRoomTypeAvailabilityAsync(
                bookingDto.RoomTypeId, 
                bookingDto.HotelId,
                bookingDto.CheckInDate, 
                bookingDto.CheckOutDate);
                
            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected room type is not available for the requested dates");
            }

            // Calculate total price
            var totalPrice = await CalculateTotalPrice(bookingDto);

            // Create booking
            var booking = new Domain.Entities.Booking
            {
                RoomTypeId = bookingDto.RoomTypeId,
                UserId = bookingDto.UserId,
                HotelId = bookingDto.HotelId,
                CheckInDate = bookingDto.CheckInDate,
                CheckOutDate = bookingDto.CheckOutDate,
                GuestCount = bookingDto.GuestCount,
                TotalPrice = totalPrice,
                Status = (int)BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);
            await _auditService.AuditActionAsync(booking.UserId, UserActionType.BookingCreated, true);
            
            return MapToDto(booking);
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(UpdateBookingDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(dto.Id) 
                ?? throw new KeyNotFoundException("Booking not found");

            // If dates changed, recalculate total price and check availability
            if (booking.CheckInDate != dto.CheckInDate ||
                booking.CheckOutDate != dto.CheckOutDate)
            {
                // Validate new dates
                if (dto.CheckInDate >= dto.CheckOutDate)
                {
                    throw new ArgumentException("Check-out date must be after check-in date");
                }

                // Check availability with new dates
                var isAvailable = await CheckRoomTypeAvailabilityAsync(
                    booking.RoomTypeId,
                    booking.HotelId, 
                    dto.CheckInDate,
                    dto.CheckOutDate,
                    booking.Id); // Exclude current booking

                if (!isAvailable)
                {
                    throw new InvalidOperationException("The room type is not available for the selected dates");
                }

                // Recalculate price
                booking.TotalPrice = await CalculateTotalPrice(new CreateBookingDto
                {
                    RoomTypeId = booking.RoomTypeId,
                    HotelId = booking.HotelId,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    GuestCount = dto.GuestCount
                });
            }

            // Check if the number of guests is within the room type capacity
            if (dto.GuestCount != booking.GuestCount)
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(booking.RoomTypeId);
                if (dto.GuestCount > roomType.Capacity)
                {
                    throw new InvalidOperationException($"This room type can only accommodate {roomType.Capacity} guests");
                }
            }

            booking.CheckInDate = dto.CheckInDate;
            booking.CheckOutDate = dto.CheckOutDate;
            booking.GuestCount = dto.GuestCount;
            booking.Status = dto.Status;
            await _bookingRepository.UpdateAsync(booking);
            await _auditService.AuditActionAsync(booking.UserId, UserActionType.BookingUpdated, true);
            
            return MapToDto(booking);
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }

            await _bookingRepository.DeleteAsync(id);
            await _auditService.AuditActionAsync(booking.UserId, UserActionType.BookingDeleted, true);
        }

        // public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        // {
        //     var bookings = await _bookingRepository.GetAllAsync(
        //         b => b.UserId == userId,
        //         include: query => query.Include(b => b.Hotel)
        //                               .Include(b => b.RoomType));
        //                               
        //     return bookings.Select(MapToDto);
        // }
        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        {
            try 
            {
                
                var bookings = await _bookingRepository.GetAllAsync(
                    b => b.UserId == userId && !b.IsDeleted
                );
                                  
                return bookings.Select(MapToDto);
            }
            catch (Exception ex)
            {
                // Fallback with no includes if error occurs
                Console.WriteLine($"Error in GetBookingsByUserIdAsync: {ex.Message}");
                var bookings = await _bookingRepository.GetAllAsync(
                    b => b.UserId == userId && !b.IsDeleted
                );
        
                return bookings.Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    RoomTypeId = b.RoomTypeId,
                    UserId = b.UserId,
                    HotelId = b.HotelId,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    GuestCount = b.GuestCount,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                });
            }
        }

        public async Task<bool> CheckRoomTypeAvailabilityAsync(
            int roomTypeId, 
            int hotelId,
            DateTime checkInDate, 
            DateTime checkOutDate,
            int? excludeBookingId = null)
        {
            // Get the room type
            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
            if (roomType == null || roomType.HotelId != hotelId)
            {
                throw new KeyNotFoundException($"Room type with ID {roomTypeId} not found in hotel {hotelId}");
            }

            // Since Room entity is removed, we assume availability.
            // TODO: Implement a new way to track room inventory.
            return true;
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found");

            booking.Status = (int)BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            
            await _bookingRepository.UpdateAsync(booking);
            
            return MapToDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _bookingRepository.GetAllAsync(
                b => b.CheckInDate >= startDate && b.CheckOutDate <= endDate);

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

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByRoomTypeIdAsync(int roomTypeId)
        {
            var bookings = await _bookingRepository.GetAllAsync(
                b => b.RoomTypeId == roomTypeId);
                                      
            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByHotelIdAsync(int hotelId)
        {
            var bookings = await _bookingRepository.GetAllAsync(
                b => b.HotelId == hotelId);
                                      
            return bookings.Select(MapToDto);
        }

        private async Task<decimal> CalculateTotalPrice(CreateBookingDto dto)
        {
            decimal totalPrice = 0;
            int nightCount = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
            
            // Try to get seasonal pricing
            var pricingRecords = await _pricingRepository.GetAllAsync(rp => 
                rp.RoomTypeId == dto.RoomTypeId &&
                rp.Date >= dto.CheckInDate.Date &&
                rp.Date < dto.CheckOutDate.Date);

            if (pricingRecords.Any())
            {
                // If we have pricing records for some or all days
                var coveredDates = pricingRecords.Select(p => p.Date.Date).ToHashSet();
                
                // Add up prices for days with specific pricing
                totalPrice += pricingRecords.Sum(p => p.Price);
                
                // If some days don't have specific pricing, use the room type's base price
                if (coveredDates.Count < nightCount)
                {
                    var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
                    
                    // For each date in the range, check if it has specific pricing
                    for (var date = dto.CheckInDate.Date; date < dto.CheckOutDate.Date; date = date.AddDays(1))
                    {
                        if (!coveredDates.Contains(date))
                        {
                            totalPrice += roomType.BasePrice;
                        }
                    }
                }
            }
            else
            {
                // If no pricing records, use the room type's base price
                var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
                totalPrice = roomType.BasePrice * nightCount;
            }
            
            return totalPrice;
        }
        
        private static BookingResponseDto MapToDto(Domain.Entities.Booking booking) => new()
        {
            Id = booking.Id,
            RoomTypeId = booking.RoomTypeId,
            UserId = booking.UserId,
            HotelId = booking.HotelId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            GuestCount = booking.GuestCount,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
        };
    }
}