using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Interfaces;

public interface IUserActionAuditService
{
    Task<UserActionAudit?> GetByIdAsync(int id);
    Task<IEnumerable<UserActionAudit>> GetAllAsync();
    Task<IEnumerable<UserActionAudit>> GetByUserIdAsync(int userId);
    Task AuditActionAsync(int userId, UserActionType actionType, bool isSuccess);
}