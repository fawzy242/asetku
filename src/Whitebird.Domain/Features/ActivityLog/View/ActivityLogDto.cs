namespace Whitebird.Domain.Features.ActivityLog.View;

public class CreateActivityLogDto
{
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
}

public class ActivityLogListDto
{
    public int LogId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}