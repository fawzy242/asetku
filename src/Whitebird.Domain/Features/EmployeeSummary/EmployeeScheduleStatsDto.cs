namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Schedule stats for employee (handover and return)
/// </summary>
public class EmployeeScheduleStatsDto
{
    public int TotalHandover { get; set; }
    public int TotalReturn { get; set; }
    public int UpcomingHandover { get; set; }
    public int UpcomingReturn { get; set; }
    public List<EmployeeScheduleItemDto> HandoverList { get; set; } = new();
    public List<EmployeeScheduleItemDto> ReturnList { get; set; } = new();
}

/// <summary>
/// Schedule item
/// </summary>
public class EmployeeScheduleItemDto
{
    public int ScheduleId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}