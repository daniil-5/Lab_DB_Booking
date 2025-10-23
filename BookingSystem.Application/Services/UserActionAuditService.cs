using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class UserActionAuditService : IUserActionAuditService
{
    private readonly IUserActionAuditRepository _userActionAuditRepository;

    public UserActionAuditService(IUserActionAuditRepository userActionAuditRepository)
    {
        _userActionAuditRepository = userActionAuditRepository;
    }

    public async Task<UserActionAudit?> GetByIdAsync(int id)
    {
        return await _userActionAuditRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<UserActionAudit>> GetAllAsync()
    {
        return await _userActionAuditRepository.GetAllAsync();
    }

    public async Task<IEnumerable<UserActionAudit>> GetByUserIdAsync(int userId)
    {
        return await _userActionAuditRepository.GetByUserIdAsync(userId);
    }

    public async Task AuditActionAsync(int userId, UserActionType actionType, bool isSuccess)
    {
        var audit = new UserActionAudit
        {
            UserId = userId,
            UserActionType = actionType,
            IsSuccess = isSuccess,
            CreatedAt = DateTime.UtcNow
        };
        await _userActionAuditRepository.AddAsync(audit);
    }
}