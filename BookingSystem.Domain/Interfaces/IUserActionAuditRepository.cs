using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Interfaces;

public interface IUserActionAuditRepository
{
    Task AddAsync(UserActionAudit entity);
}
