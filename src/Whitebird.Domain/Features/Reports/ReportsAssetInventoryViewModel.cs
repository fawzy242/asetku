using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for asset inventory report
/// </summary>
public class ReportsAssetInventoryViewModel
{
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

    [Display(Name = "Current Status")]
    public string CurrentStatus { get; set; } = default!;

    [Display(Name = "Condition")]
    public string? AssetConditionName { get; set; }

    [Display(Name = "Condition (Purchase)")]
    public string? AssetConditionPurchaseName { get; set; }

    [Display(Name = "Office")]
    public string? OfficeName { get; set; }

    [Display(Name = "Category")]
    public string? CategoryName { get; set; }

    [Display(Name = "Supplier")]
    public string? SupplierName { get; set; }

    [Display(Name = "Current Holder")]
    public string? CurrentHolderName { get; set; }

    [Display(Name = "Purchase Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Purchase Price")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Warranty Expiry")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? WarrantyExpiryDate { get; set; }

    [Display(Name = "Last Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? LastMaintenanceDate { get; set; }

    [Display(Name = "Next Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? NextMaintenanceDate { get; set; }

    [Display(Name = "Hostname")]
    public string? Hostname { get; set; }

    [Display(Name = "IP Address")]
    public string? IpAddress { get; set; }

    [Display(Name = "Operasional Office")]
    public string? OperasionalOffice { get; set; }

    [Display(Name = "Residual Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? ResidualValue { get; set; }

    [Display(Name = "Useful Life (Years)")]
    public int? UsefulLife { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}