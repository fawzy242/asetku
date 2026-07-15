namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Timeline entry for asset tracking (checkpoint)
/// </summary>
public class AssetTimelineEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PreviousHolder { get; set; }
    public string? NewHolder { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Notes { get; set; }
}