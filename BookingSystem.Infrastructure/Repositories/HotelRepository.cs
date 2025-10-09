// HotelRepository.cs
using Dapper;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly DapperDbContext _context;
    public HotelRepository(DapperDbContext context) { _context = context; }

    public async Task<Hotel> GetByIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Hotel>(
            "select * from hotels where id=@id and is_deleted=false", new { id });
    }

    public async Task<IEnumerable<Hotel>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<Hotel>("select * from hotels where is_deleted=false");
    }

    public async Task<IEnumerable<Hotel>> GetAllAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate) 
    {
        var allHotels = await GetAllAsync();
        return allHotels.Where(predicate.Compile());
    }

    public Task<Hotel> GetByIdAsync(int id, Func<IQueryable<Hotel>, IQueryable<Hotel>> include) 
        => throw new NotSupportedException();

    public Task<IEnumerable<Hotel>> GetAllAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate = null, Func<IQueryable<Hotel>, IQueryable<Hotel>> include = null) 
        => throw new NotSupportedException();

    public async Task AddAsync(Hotel entity)
    {
        using var conn = _context.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>(
@"insert into hotels (name, description, location, rating, base_price, created_at, is_deleted)
 values (@Name, @Description, @Location, @Rating, @BasePrice, @CreatedAt, false)
 returning id", entity);
        entity.Id = id;
    }

    public async Task UpdateAsync(Hotel entity)
    {
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(
@"update hotels
   set name=@Name, description=@Description, location=@Location, rating=@Rating, base_price=@BasePrice, updated_at=@UpdatedAt
 where id=@Id and is_deleted=false", entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync("update hotels set is_deleted=true, updated_at=now() where id=@id", new { id });
    }

    public async Task<Hotel> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate)
        => (await GetAllAsync()).FirstOrDefault(predicate.Compile());

    public Task<Hotel> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate, Func<IQueryable<Hotel>, IQueryable<Hotel>> include)
        => throw new NotSupportedException();

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate = null)
    {
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("select count(*) from hotels where is_deleted=false");
    }

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Hotel, bool>> predicate, Func<IQueryable<Hotel>, IQueryable<Hotel>> include)
        => throw new NotSupportedException();

    public IQueryable<Hotel> GetQueryable() => throw new NotSupportedException();
    
    public async Task<(IEnumerable<Hotel> hotels, int totalCount)> SearchHotelsAsync(
        System.Linq.Expressions.Expression<Func<Hotel, bool>> filter = null,
        Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>> orderBy = null,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeRoomTypes = false,
        bool includePhotos = false)
    {
        using var conn = _context.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;

        var items = await conn.QueryAsync<Hotel>(
            "select * from hotels where is_deleted=false order by id limit @pageSize offset @offset",
            new { pageSize, offset });

        var total = await conn.ExecuteScalarAsync<int>(
            "select count(*) from hotels where is_deleted=false");

        return (items, total);
    }

    public async Task<IEnumerable<Hotel>> GetHotelsWithDetailsAsync(
        System.Linq.Expressions.Expression<Func<Hotel, bool>> filter = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var (hotels, _) = await SearchHotelsAsync(null, null, pageNumber, pageSize);
        return hotels;
    }

    public async Task<Hotel> GetHotelWithDetailsAsync(int id)
    {
        using var conn = _context.CreateConnection();

        var hotel = await conn.QuerySingleOrDefaultAsync<Hotel>(
            "select * from hotels where id=@id and is_deleted=false", new { id });
        if (hotel == null) return null;

        var roomTypes = await conn.QueryAsync<RoomType>(
            "select * from room_types where hotel_id=@hotelId and is_deleted=false",
            new { hotelId = id });
        hotel.RoomTypes = roomTypes.ToList();

        var photos = await conn.QueryAsync<HotelPhoto>(
            "select * from hotel_photos where hotel_id=@hotelId and is_deleted=false",
            new { hotelId = id });
        hotel.Photos = photos.ToList();

        return hotel;
    }

    public Task<IEnumerable<Hotel>> SearchHotelsByAvailabilityAsync(
        string location, DateTime checkIn, DateTime checkOut, int guests,
        int pageNumber = 1, int pageSize = 10)
    {
        return Task.FromResult<IEnumerable<Hotel>>(Array.Empty<Hotel>());
    }
}