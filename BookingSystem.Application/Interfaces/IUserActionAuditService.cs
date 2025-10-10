using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Interfaces;

public interface IUserActionAuditService
{
    Task AuditActionAsync(int userId, UserActionType actionType, bool isSuccess);
}
