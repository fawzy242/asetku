namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// DTO for schedule statistics on dashboard
/// </summary>
public class ScheduleStatsDto
{
    public int TotalHandover { get; set; }
    public int TotalReturn { get; set; }
    public int TotalMaintenance { get; set; }
    public int UpcomingHandover { get; set; }
    public int UpcomingReturn { get; set; }
    public int UpcomingMaintenance { get; set; }
}