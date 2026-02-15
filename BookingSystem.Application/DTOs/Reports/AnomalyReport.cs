namespace BookingSystem.Application.DTOs.Reports;

public class AnomalyReport
{
    public int UserId { get; set; }
    public string AnomalyDescription { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
