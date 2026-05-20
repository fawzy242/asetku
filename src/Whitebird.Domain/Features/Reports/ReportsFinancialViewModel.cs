using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for financial report
/// </summary>
public class ReportsFinancialViewModel
{
    [Display(Name = "Asset ID")]
    public int AssetId { get; set; }

    [Display(Name = "Asset Code")]
    public string AssetCode { get; set; } = default!;

    [Display(Name = "Asset Name")]
    public string AssetName { get; set; } = default!;

    [Display(Name = "Category")]
    public string? CategoryName { get; set; }

    [Display(Name = "Supplier")]
    public string? SupplierName { get; set; }

    [Display(Name = "Purchase Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Purchase Price")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Invoice Number")]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Warranty Period (Months)")]
    public int? WarrantyPeriod { get; set; }

    [Display(Name = "Warranty Expiry")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? WarrantyExpiryDate { get; set; }

    [Display(Name = "Is Warranty Expired")]
    public string IsWarrantyExpired { get; set; } = "No";

    [Display(Name = "Residual Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? ResidualValue { get; set; }

    [Display(Name = "Useful Life (Years)")]
    public int? UsefulLife { get; set; }

    [Display(Name = "Depreciation Start")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? DepreciationStartDate { get; set; }

    [Display(Name = "Annual Depreciation")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? AnnualDepreciation { get; set; }

    [Display(Name = "Current Book Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? CurrentBookValue { get; set; }

    [Display(Name = "Maintenance Count")]
    public int MaintenanceCount { get; set; }

    [Display(Name = "Total Maintenance Cost")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? TotalMaintenanceCost { get; set; }

    [Display(Name = "Total Cost of Ownership")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? TotalCostOfOwnership { get; set; }

    [Display(Name = "Office")]
    public string? OfficeName { get; set; }

    [Display(Name = "Condition (Purchase)")]
    public string? AssetConditionPurchaseName { get; set; }
}