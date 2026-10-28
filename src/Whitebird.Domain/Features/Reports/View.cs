using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// View model for asset inventory report
/// </summary>
public class ReportsAssetInventoryViewModel
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

    [Display(Name = "Hostname")]
    public string? Hostname { get; set; }

    [Display(Name = "IP Address")]
    public string? IpAddress { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

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
    public string? ConditionName { get; set; }

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
    public DateTime? ExpectedLoanReturnDate { get; set; }

    [Display(Name = "Office")]
    public string? OfficeName { get; set; }

    [Display(Name = "Employee ID")]
    public int? EmployeeId { get; set; }
}

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

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

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