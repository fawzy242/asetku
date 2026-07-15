namespace Whitebird.Domain.Features.Schedule;

public class ScheduleGenerateResultDto
{
    public int TotalGenerated { get; set; }
    public int TotalSkipped { get; set; }
    public List<DateTime> GeneratedDates { get; set; } = new();
    public List<DateTime> SkippedDates { get; set; } = new();
    public string? Message { get; set; }
}