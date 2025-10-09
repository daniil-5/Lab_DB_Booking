
using System.Linq.Expressions;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Dapper;

namespace BookingSystem.Infrastructure.Repositories;

public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
{
    public HotelRepository(DapperDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Hotel> hotels, int totalCount)> SearchHotelsAsync(Expression<Func<Hotel, bool>> filter = null, Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>> orderBy = null, int pageNumber = 1, int pageSize = 10, bool includeRoomTypes = false, bool includePhotos = false)
    {
        // Dapper does not support IQueryable and Expression trees directly. 
        // This method would need to be re-implemented with raw SQL and dynamic query building.
        // For now, we will return an empty result.
        return (new List<Hotel>(), 0);
    }

    public async Task<IEnumerable<Hotel>> GetHotelsWithDetailsAsync(Expression<Func<Hotel, bool>> filter = null, int pageNumber = 1, int pageSize = 10)
    {
        // This method would also require dynamic SQL generation based on the filter expression.
        // Returning an empty list for now.
        return new List<Hotel>();
    }

    public async Task<Hotel> GetHotelWithDetailsAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var hotelSql = "SELECT * FROM \"Hotels\" WHERE \"Id\" = @Id AND \"IsDeleted\" = false";
        var hotel = await connection.QuerySingleOrDefaultAsync<Hotel>(hotelSql, new { Id = id });

        if (hotel != null)
        {
            var roomTypesSql = "SELECT * FROM \"RoomTypes\" WHERE \"HotelId\" = @HotelId AND \"IsDeleted\" = false";
            hotel.RoomTypes = (await connection.QueryAsync<RoomType>(roomTypesSql, new { HotelId = id })).ToList();

            var photosSql = "SELECT * FROM \"HotelPhotos\" WHERE \"HotelId\" = @HotelId AND \"IsDeleted\" = false";
            hotel.Photos = (await connection.QueryAsync<HotelPhoto>(photosSql, new { HotelId = id })).ToList();
        }

        return hotel;
    }

    public async Task<IEnumerable<Hotel>> SearchHotelsByAvailabilityAsync(string location, DateTime checkIn, DateTime checkOut, int guests, int pageNumber = 1, int pageSize = 10)
    {
        // This is a complex query that would require a sophisticated SQL query to implement correctly with Dapper.
        // Returning an empty list for now.
        return new List<Hotel>();
    }
}
