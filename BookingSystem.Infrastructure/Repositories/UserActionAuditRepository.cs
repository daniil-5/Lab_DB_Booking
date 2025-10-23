using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Dapper;

namespace BookingSystem.Infrastructure.Repositories;

public class UserActionAuditRepository : IUserActionAuditRepository
{
    private readonly DapperDbContext _context;

    public UserActionAuditRepository(DapperDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserActionAudit entity)
    {
        using var connection = _context.CreateConnection();
        var sql = "INSERT INTO user_action_audit (user_id, user_action_type, is_success, created_at, updated_at, is_deleted) VALUES (@UserId, @UserActionType, @IsSuccess, @CreatedAt, @UpdatedAt, @IsDeleted)";
        await connection.ExecuteAsync(sql, new { entity.UserId, UserActionType = entity.UserActionType.ToString(), entity.IsSuccess, entity.CreatedAt, entity.UpdatedAt, entity.IsDeleted });
    }

    public async Task<UserActionAudit?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM user_action_audit WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<UserActionAudit>(sql, new { Id = id });
    }

    public async Task<IEnumerable<UserActionAudit>> GetAllAsync()
    {        
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM user_action_audit";
        return await connection.QueryAsync<UserActionAudit>(sql);
    }

    public async Task<IEnumerable<UserActionAudit>> GetByUserIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM user_action_audit WHERE user_id = @UserId";
        return await connection.QueryAsync<UserActionAudit>(sql, new { UserId = userId });
    }
}
