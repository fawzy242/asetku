using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Schedule;

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