using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Interfaces;

public interface ILogRepository
{
    Task AddLogAsync(LogEntity log);
    Task<IEnumerable<LogEntity>> GetLogsAsync(DateTime? startDate, DateTime? endDate, int? userId, string? eventType);
}