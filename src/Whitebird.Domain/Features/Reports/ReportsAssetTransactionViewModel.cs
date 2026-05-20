using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for asset transaction report
/// </summary>
public class ReportsAssetTransactionViewModel
{
    [Display(Name = "Employee Code")]
    public string? EmployeeCode { get; set; }

    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    [Display(Name = "Category Name")]
    public string? CategoryName { get; set; }

    [Display(Name = "Category ID")]
    public int CategoryId { get; set; }

    [Display(Name = "Asset Name")]
    public string AssetName { get; set; } = default!;

    [Display(Name = "Asset Code")]
    public string AssetCode { get; set; } = default!;

    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [Display(Name = "Model")]
    public string? Model { get; set; }

    [Display(Name = "Condition")]
    public string? AssetConditionName { get; set; }

    [Display(Name = "Purchase Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Transaction Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime TransactionDate { get; set; }

    [Display(Name = "Transaction Type")]
    public string TransactionTypeName { get; set; } = default!;

    [Display(Name = "Approval Status")]
    public string ApprovalStatus { get; set; } = default!;

    [Display(Name = "Purchase Price")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Expected Return Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? ExpectedReturnDate { get; set; }

    [Display(Name = "Actual Return Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? ActualReturnDate { get; set; }

    [Display(Name = "Office")]
    public string? OfficeName { get; set; }
}