using BookingSystem.Application.DTOs.Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Application.Interfaces;

public interface IReportingService
{
    Task<IEnumerable<UserActivityReport>> GetUserActivityReportAsync(string period);
    Task<IEnumerable<TopUserReport>> GetTopUsersReportAsync();
    Task<IEnumerable<OperationDistributionReport>> GetOperationDistributionReportAsync();
    Task<IEnumerable<TimeSeriesReport>> GetTimeSeriesReportAsync();
    Task<IEnumerable<AnomalyReport>> GetAnomalyReportAsync();
}
