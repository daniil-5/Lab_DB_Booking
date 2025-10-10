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
}
