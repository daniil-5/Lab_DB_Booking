using System.Linq.Expressions;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories;

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
        IQueryable<Hotel> query = _context.Set<Hotel>().Where(h => !h.IsDeleted);

        // Include related entities if requested
        if (includeRoomTypes)
            query = query.Include(h => h.RoomTypes);
                
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
}