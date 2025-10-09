using Dapper;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class BookingRepository : IRepository<Booking>
{
    private readonly DapperDbContext _context;
    public BookingRepository(DapperDbContext context) { _context = context; }

    public async Task<Booking> GetByIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Booking>(
            "select * from bookings where id = @id and is_deleted = false", new { id });
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "select * from bookings where is_deleted = false");
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate)
    {
        // Since we already filter by is_deleted=false in SQL, we can just return all non-deleted bookings
        // and let the predicate filter in memory for simple cases like !b.IsDeleted
        var allBookings = await GetAllAsync();
        return allBookings.Where(predicate.Compile());
    }

    public Task<Booking> GetByIdAsync(int id, Func<IQueryable<Booking>, IQueryable<Booking>> include)
        => throw new NotSupportedException();

    public Task<IEnumerable<Booking>> GetAllAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate = null, Func<IQueryable<Booking>, IQueryable<Booking>> include = null)
        => throw new NotSupportedException();

    public async Task AddAsync(Booking entity)
    {
        using var conn = _context.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>(
@"insert into bookings (user_id, room_type_id, check_in_date, check_out_date, status, total_price, created_at, is_deleted)
 values (@UserId, @RoomTypeId, @CheckInDate, @CheckOutDate, @Status, @TotalPrice, @CreatedAt, false)
 returning id", entity);
        entity.Id = id;
    }

    public async Task UpdateAsync(Booking entity)
    {
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(
@"update bookings
   set user_id=@UserId, room_type_id=@RoomTypeId, check_in_date=@CheckInDate, check_out_date=@CheckOutDate,
       status=@Status, total_price=@TotalPrice, updated_at=@UpdatedAt
 where id=@Id and is_deleted=false", entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync("update bookings set is_deleted=true, updated_at=now() where id=@id", new { id });
    }

    public async Task<Booking> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate)
        => (await GetAllAsync()).FirstOrDefault(predicate.Compile());

    public Task<Booking> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate, Func<IQueryable<Booking>, IQueryable<Booking>> include)
        => throw new NotSupportedException();

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate = null)
    {
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("select count(*) from bookings where is_deleted=false");
    }

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Booking, bool>> predicate, Func<IQueryable<Booking>, IQueryable<Booking>> include)
        => throw new NotSupportedException();

    public IQueryable<Booking> GetQueryable() => throw new NotSupportedException();
}