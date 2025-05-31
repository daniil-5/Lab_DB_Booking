// using System.Linq.Expressions;
// using BookingSystem.Domain.Entities;
// using BookingSystem.Domain.Interfaces;
// using BookingSystem.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
//
// namespace BookingSystem.Infrastructure.Repositories;
//
// public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
// {
//     private readonly AppDbContext _context;
//
//     public HotelRepository(AppDbContext context) : base(context)
//     {
//         _context = context;
//     }
//
//     public async Task<(IEnumerable<Hotel> hotels, int totalCount)> SearchHotelsAsync(
//         Expression<Func<Hotel, bool>> filter = null,
//         Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>> orderBy = null,
//         int pageNumber = 1,
//         int pageSize = 10,
//         bool includeRoomTypes = false,
//         bool includePhotos = false)
//     {
//         // Start with all hotels that aren't deleted
//         IQueryable<Hotel> query = _context.Set<Hotel>().Where(h => !h.IsDeleted);
//
//         // Include related entities if requested
//         if (includeRoomTypes)
//             query = query.Include(h => h.RoomTypes);
//                 
//         if (includePhotos)
//             query = query.Include(h => h.Photos.Where(p => !p.IsDeleted));
//
//         // Apply the filter if provided
//         if (filter != null)
//             query = query.Where(filter);
//
//         // Get total count for pagination info
//         var totalCount = await query.CountAsync();
//
//         // Apply ordering
//         if (orderBy != null)
//             query = orderBy(query);
//         else
//             query = query.OrderByDescending(h => h.Rating); // Default sort
//
//         // Apply pagination
//         var hotels = await query
//             .Skip((pageNumber - 1) * pageSize)
//             .Take(pageSize)
//             .ToListAsync();
//
//         return (hotels, totalCount);
//     }
// }
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Enums;


namespace BookingSystem.Infrastructure.Repositories
{
    public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
    {
        private readonly AppDbContext _context;

        public HotelRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Hotel> hotels, int totalCount)> SearchHotelsAsync(
            Expression<Func<Hotel, bool>> filter = null,
            Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>> orderBy = null,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeRoomTypes = false,
            bool includePhotos = false)
        {
            // Start with all hotels that aren't deleted
            IQueryable<Hotel> query = _context.Hotels.Where(h => !h.IsDeleted);

            // Include related entities if requested
            if (includeRoomTypes)
                query = query.Include(h => h.RoomTypes.Where(rt => !rt.IsDeleted));
                
            if (includePhotos)
                query = query.Include(h => h.Photos.Where(p => !p.IsDeleted));

            // Apply the filter if provided
            if (filter != null)
                query = query.Where(filter);

            // Get total count for pagination info
            var totalCount = await query.CountAsync();

            // Apply ordering
            if (orderBy != null)
                query = orderBy(query);
            else
                query = query.OrderByDescending(h => h.Rating); // Default sort

            // Apply pagination
            var hotels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (hotels, totalCount);
        }
        
        // New methods to match functionality in the generic repository
        
        public async Task<IEnumerable<Hotel>> GetHotelsWithDetailsAsync(
            Expression<Func<Hotel, bool>> filter = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            IQueryable<Hotel> query = _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.Photos)
                .Where(h => !h.IsDeleted);
                
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            return await query
                .OrderByDescending(h => h.Rating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        
        public async Task<Hotel> GetHotelWithDetailsAsync(int id)
        {
            return await _context.Hotels
                .Include(h => h.RoomTypes.Where(rt => !rt.IsDeleted))
                .Include(h => h.Photos.Where(p => !p.IsDeleted))
                .Include(h => h.RoomTypes)
                    .ThenInclude(rt => rt.Pricing.Where(p => 
                        p.Date >= DateTime.UtcNow && 
                        p.Date <= DateTime.UtcNow.AddDays(90)))
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }
        
        public async Task<IEnumerable<Hotel>> SearchHotelsByAvailabilityAsync(
            string location, 
            DateTime checkIn, 
            DateTime checkOut, 
            int guests, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            // Validate dates
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-out date must be after check-in date");
                
            if (checkIn.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Check-in date cannot be in the past");
                
            var query = _context.Hotels
                .Include(h => h.RoomTypes.Where(rt => !rt.IsDeleted && rt.Capacity >= guests))
                .Include(h => h.Photos.Where(p => !p.IsDeleted))
                .Where(h => !h.IsDeleted);
                
            // Filter by location (city, country, address)
            if (!string.IsNullOrEmpty(location))
            {
                location = location.ToLower();
                query = query.Where(h => 
                    h.Location.ToLower().Contains(location));
            }
            
            // We need to ensure the hotel has available room types for the requested dates
            var hotels = await query.ToListAsync();
            
            var availableHotels = new List<Hotel>();
            
            foreach (var hotel in hotels)
            {
                // Check if the hotel has any room type with available capacity
                bool hasAvailableRoomType = false;
                
                foreach (var roomType in hotel.RoomTypes)
                {
                    // Count existing bookings for this room type during the requested period
                    var bookingCount = await _context.Bookings.CountAsync(b => 
                        b.RoomTypeId == roomType.Id &&
                        b.Status != (int)BookingStatus.Cancelled &&
                        b.CheckInDate < checkOut &&
                        b.CheckOutDate > checkIn);
                        
                    // Count rooms of this type
                    var roomCount = await _context.Rooms.CountAsync(r => 
                        r.RoomTypeId == roomType.Id && 
                        !r.IsDeleted && 
                        r.IsAvailable);
                        
                    if (roomCount > bookingCount)
                    {
                        hasAvailableRoomType = true;
                        break;
                    }
                }
                
                if (hasAvailableRoomType)
                {
                    availableHotels.Add(hotel);
                }
            }
            
            // Apply pagination
            return availableHotels
                .OrderByDescending(h => h.Rating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}