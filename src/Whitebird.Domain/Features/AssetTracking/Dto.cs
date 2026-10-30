using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// DTO for asset lookup/dropdown in Asset Tracking menu
/// </summary>
public class AssetTrackingGetDataDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Grid request for Asset Tracking
/// </summary>
public class AssetTrackingGridRequest : GridRequestBase
{
    /// <summary>
    /// Asset ID (required, passed in path)
    /// </summary>
    public int AssetId { get; set; }

    /// <summary>
    /// P3-2: explicit schedule-type selector for the schedule-stats endpoint.
    /// Supported values (case-insensitive): "handover", "return", "maintenance".
    /// Falls back to <see cref="GridRequestBase.Search"/> for backward
    /// compatibility when this is null — existing clients that used the query
    /// parameter "search" as a type selector continue to work unchanged.
    /// </summary>
    public string? ScheduleType { get; set; }
}

/// <summary>
/// Schedule stats for a specific asset (grid data)
/// </summary>
public class AssetTrackingScheduleStatsDto
{
    public int ScheduleId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public int? RelatedTransactionId { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Employee history for a specific asset (grid data)
/// </summary>
public class AssetTrackingEmployeeHistoryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public int TotalTransactions { get; set; }
    public int AssetsHeld { get; set; }
}

/// <summary>
/// Transaction type composition for a specific asset (chart data)
/// </summary>
public class AssetTrackingTransactionTypeStatsDto
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for transaction type stats
/// </summary>
public class AssetTrackingTransactionTypeStatsResponse
{
    public int TotalTransactions { get; set; }
    public List<AssetTrackingTransactionTypeStatsDto> Data { get; set; } = new();
}