using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services;

public class UserActionAuditService : IUserActionAuditService
{
    private readonly IUserActionAuditRepository _auditRepository;
    private readonly ILogger<UserActionAuditService> _logger;

    public UserActionAuditService(IUserActionAuditRepository auditRepository, ILogger<UserActionAuditService> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task AuditActionAsync(int userId, UserActionType actionType, bool isSuccess)
    {
        _logger.LogInformation("Auditing action {ActionType} for user {UserId}. Success: {IsSuccess}", actionType, userId, isSuccess);
        try
        {
            var audit = new UserActionAudit
            {
                UserId = userId,
                UserActionType = actionType,
                IsSuccess = isSuccess,
                CreatedAt = DateTime.UtcNow
            };

            await _auditRepository.AddAsync(audit);
            _logger.LogInformation("Successfully audited action {ActionType} for user {UserId}.", actionType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auditing action {ActionType} for user {UserId}.", actionType, userId);
        }
    }
}
