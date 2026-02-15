using BookingSystem.Application.DTOs.Reports;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Application.Services;

public class ReportingService : IReportingService
{
    private readonly ILogRepository _logRepository;

    public ReportingService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<IEnumerable<UserActivityReport>> GetUserActivityReportAsync(string period)
    {
        var result = await _logRepository.GetUserActivityReportAsync(period);
        return result.Select(r => new UserActivityReport
        {
            Period = r.Id.ToString(),
            TotalActions = r.TotalActions
        });
    }

    public async Task<IEnumerable<TopUserReport>> GetTopUsersReportAsync()
    {
        var result = await _logRepository.GetTopUsersReportAsync();
        return result.Select(r => new TopUserReport
        {
            UserId = r.UserId,
            TotalActions = r.TotalActions
        });
    }

    public async Task<IEnumerable<OperationDistributionReport>> GetOperationDistributionReportAsync()
    {
        var result = await _logRepository.GetOperationDistributionReportAsync();
        return result.Select(r => new OperationDistributionReport
        {
            ActionType = r.ActionType,
            Count = r.Count
        });
    }

    public async Task<IEnumerable<TimeSeriesReport>> GetTimeSeriesReportAsync()
    {
        var result = await _logRepository.GetTimeSeriesReportAsync();
        return result.Select(r => new TimeSeriesReport
        {
            Date = new System.DateTime( (int)((dynamic)r.Id).year, (int)((dynamic)r.Id).month, (int)((dynamic)r.Id).day),
            TotalActions = r.TotalActions
        });
    }

    public Task<IEnumerable<AnomalyReport>> GetAnomalyReportAsync()
    {
        return Task.FromResult(Enumerable.Empty<AnomalyReport>());
    }
}
