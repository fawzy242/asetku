using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for employee asset report
/// </summary>
public class ReportsEmployeeAssetViewModel
{
    [Display(Name = "Employee ID")]
    public int EmployeeId { get; set; }

    [Display(Name = "Employee Code")]
    public string EmployeeCode { get; set; } = default!;

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = default!;

    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    [Display(Name = "Position")]
    public string? PositionName { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Employment Status")]
    public string? EmploymentStatusName { get; set; }

    [Display(Name = "Office")]
    public string? OfficeName { get; set; }

    [Display(Name = "Asset ID")]
    public int AssetId { get; set; }

    [Display(Name = "Asset Code")]
    public string AssetCode { get; set; } = default!;

    [Display(Name = "Asset Name")]
    public string AssetName { get; set; } = default!;

    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [Display(Name = "Model")]
    public string? Model { get; set; }

    [Display(Name = "Category")]
    public string? CategoryName { get; set; }

    [Display(Name = "Condition")]
    public string? AssetConditionName { get; set; }

    [Display(Name = "Assignment Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? AssignmentDate { get; set; }

    [Display(Name = "Expected Return")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? ExpectedReturnDate { get; set; }

    [Display(Name = "Association Type")]
    public string AssociationType { get; set; } = default!;

    [Display(Name = "Is Overdue")]
    public string IsOverdue { get; set; } = "No";

    [Display(Name = "Last Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? LastMaintenanceDate { get; set; }

    [Display(Name = "Next Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? NextMaintenanceDate { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}