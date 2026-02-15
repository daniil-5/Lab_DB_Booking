using System;

namespace BookingSystem.Application.DTOs.Reports;

public class TimeSeriesReport
{
    public DateTime Date { get; set; }
    public int TotalActions { get; set; }
}
