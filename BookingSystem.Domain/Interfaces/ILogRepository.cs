using BookingSystem.Domain.Entities;
using BookingSystem.Domain.DTOs.Reports;

namespace BookingSystem.Domain.Interfaces;

public interface ILogRepository
{
    Task AddLogAsync(LogEntity log);
    Task<IEnumerable<LogEntity>> GetLogsAsync(DateTime? startDate, DateTime? endDate, int? userId, string? eventType);
    Task<IEnumerable<UserActivity>> GetUserActivityReportAsync(string period);
    Task<IEnumerable<TopUser>> GetTopUsersReportAsync();
    Task<IEnumerable<OperationDistribution>> GetOperationDistributionReportAsync();
    Task<IEnumerable<TimeSeries>> GetTimeSeriesReportAsync();
}