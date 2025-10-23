using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Interfaces;

public interface IUserActionAuditRepository
{
    Task AddAsync(UserActionAudit entity);
    Task<UserActionAudit?> GetByIdAsync(int id);
    Task<IEnumerable<UserActionAudit>> GetAllAsync();
    Task<IEnumerable<UserActionAudit>> GetByUserIdAsync(int userId);
}
