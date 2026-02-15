namespace BookingSystem.Application.DTOs.Reports;

public class UserActivityReport
{
    public string Period { get; set; } = null!;
    public int TotalActions { get; set; }
}
