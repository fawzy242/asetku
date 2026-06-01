namespace Whitebird.Domain.Features.ActivityLog;

public class ActivityLogListViewModel
{
    public int LogId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}

public class ActivityLogDetailViewModel
{
    public int LogId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}