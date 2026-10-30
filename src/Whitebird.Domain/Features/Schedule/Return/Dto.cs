using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

/// <summary>
/// Grid request for ScheduleReturn.
/// Fields used by ScheduleReturnReps.GetPagedListAsync: AssetId, EmployeeId, DateFrom, DateTo.
/// Page/PageSize/Search/IsActive/SortBy/SortDescending come from GridRequestBase.
/// </summary>
public class ScheduleReturnGridRequest : GridRequestBase
{
    public int? AssetId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ScheduleReturnCreateDto
{
    [Required(ErrorMessage = "ScheduleDate is required")]
    public DateTime ScheduleDate { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "EmployeeId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be greater than 0")]
    public int EmployeeId { get; set; }
}

public class ScheduleReturnUpdateDto
{
    [Required(ErrorMessage = "ScheduleDate is required")]
    public DateTime ScheduleDate { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "EmployeeId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be greater than 0")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}

public class ScheduleReturnGenerateDto
{
    [Required(ErrorMessage = "DateFrom is required")]
    public DateTime DateFrom { get; set; }

    [Required(ErrorMessage = "DateTo is required")]
    public DateTime DateTo { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "EmployeeId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be greater than 0")]
    public int EmployeeId { get; set; }
}

/// <summary>
/// Result of a ScheduleReturn generation run.
/// Module-specific type per the standardized per-module Dto organization.
/// </summary>
public class ScheduleReturnGenerateResultDto
{
    public int TotalGenerated { get; set; }
    public int TotalSkipped { get; set; }
    public List<DateTime> GeneratedDates { get; set; } = new();
    public List<DateTime> SkippedDates { get; set; } = new();
    public string? Message { get; set; }
}