using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for maintenance report
/// </summary>
public class ReportsMaintenanceViewModel
{
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

    [Display(Name = "Current Holder")]
    public string? CurrentHolderName { get; set; }

    [Display(Name = "Current Office")]
    public string? OfficeName { get; set; }

    [Display(Name = "Condition")]
    public string? AssetConditionName { get; set; }

    [Display(Name = "Maintenance Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? MaintenanceDate { get; set; }

    [Display(Name = "Maintenance Type")]
    public string? MaintenanceTypeName { get; set; }

    [Display(Name = "Maintenance Cost")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? MaintenanceCost { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Total Maintenance Cost")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? TotalMaintenanceCost { get; set; }

    [Display(Name = "Maintenance Count")]
    public int MaintenanceCount { get; set; }

    [Display(Name = "Current Status")]
    public string CurrentStatus { get; set; } = default!;
}