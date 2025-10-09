
using System.Linq.Expressions;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Dapper;

namespace BookingSystem.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(DapperDbContext context) : base(context)
    {
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM \"Users\" WHERE \"Email\" = @Email AND \"IsDeleted\" = false";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<(IEnumerable<User> users, int totalCount)> SearchUsersAsync(Expression<Func<User, bool>> filter = null, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null, int pageNumber = 1, int pageSize = 10)
    {
        // Dapper does not support IQueryable and Expression trees directly. 
        // This method would need to be re-implemented with raw SQL and dynamic query building.
        // For now, we will return an empty result.
        return (new List<User>(), 0);
    }

    public async Task<User> GetUserWithDetailsAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        var userSql = "SELECT * FROM \"Users\" WHERE \"Id\" = @UserId AND \"IsDeleted\" = false";
        var user = await connection.QuerySingleOrDefaultAsync<User>(userSql, new { UserId = userId });

        if (user != null)
        {
            var bookingsSql = "SELECT * FROM \"Bookings\" WHERE \"UserId\" = @UserId AND \"IsDeleted\" = false";
            user.Bookings = (await connection.QueryAsync<Booking>(bookingsSql, new { UserId = userId })).ToList();
        }

        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT COUNT(1) FROM \"Users\" WHERE \"Email\" = @Email AND \"IsDeleted\" = false";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
    }
}
