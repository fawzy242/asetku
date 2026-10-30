using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

/// <summary>
/// Grid request for ScheduleMaintenance.
/// Fields used by ScheduleMaintenanceReps.GetPagedListAsync: AssetId, MaintenanceType, DateFrom, DateTo.
/// Page/PageSize/Search/IsActive/SortBy/SortDescending come from GridRequestBase.
/// </summary>
public class ScheduleMaintenanceGridRequest : GridRequestBase
{
    public int? AssetId { get; set; }
    public int? MaintenanceType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ScheduleMaintenanceCreateDto
{
    [Required(ErrorMessage = "ScheduleDate is required")]
    public DateTime ScheduleDate { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "MaintenanceType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MaintenanceType must be greater than 0")]
    public int MaintenanceType { get; set; }
}

public class ScheduleMaintenanceUpdateDto
{
    [Required(ErrorMessage = "ScheduleDate is required")]
    public DateTime ScheduleDate { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "MaintenanceType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MaintenanceType must be greater than 0")]
    public int MaintenanceType { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}

public class ScheduleMaintenanceGenerateDto
{
    [Required(ErrorMessage = "DateFrom is required")]
    public DateTime DateFrom { get; set; }

    [Required(ErrorMessage = "DateTo is required")]
    public DateTime DateTo { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "MaintenanceType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MaintenanceType must be greater than 0")]
    public int MaintenanceType { get; set; }
}

/// <summary>
/// Result of a ScheduleMaintenance generation run.
/// Module-specific type per the standardized per-module Dto organization.
/// </summary>
public class ScheduleMaintenanceGenerateResultDto
{
    public int TotalGenerated { get; set; }
    public int TotalSkipped { get; set; }
    public List<DateTime> GeneratedDates { get; set; } = new();
    public List<DateTime> SkippedDates { get; set; } = new();
    public string? Message { get; set; }
}