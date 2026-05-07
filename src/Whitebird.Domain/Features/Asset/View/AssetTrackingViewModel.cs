namespace Whitebird.Domain.Features.Asset.View;

/// <summary>
/// Comprehensive asset tracking and history view model
/// </summary>
public class AssetTrackingViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CurrentStatus { get; set; } = default!;
    public string? CategoryName { get; set; }
    public string? CurrentHolderName { get; set; }
    public string? CurrentLocation { get; set; }
    public string? Condition { get; set; }

    public bool IsOnLoan { get; set; }
    public bool IsInMaintenance { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? LoanDueDate { get; set; }

    public int TotalTransactions { get; set; }
    public List<AssetTimelineEntry> Timeline { get; set; } = new();
}

public class AssetTimelineEntry
{
    public DateTime Date { get; set; }
    public string ActivityType { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PreviousHolder { get; set; }
    public string? NewHolder { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Notes { get; set; }
}