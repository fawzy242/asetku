using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports.View;

public class ReportsAssetTransactionViewModel
{
    [Display(Name = "Employee Code")]
    public string? EmployeeCode { get; set; }

    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Department")]
    public string? Department { get; set; }

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
    public string? Condition { get; set; }

    [Display(Name = "Purchase Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Transaction Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime TransactionDate { get; set; }

    [Display(Name = "Transaction Type")]
    public string TransactionType { get; set; } = default!;

    [Display(Name = "Transaction Status")]
    public string TransactionStatus { get; set; } = default!;

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
}

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

    [Display(Name = "Status")]
    public string Status { get; set; } = default!;

    [Display(Name = "Condition")]
    public string? Condition { get; set; }

    [Display(Name = "Location")]
    public string? Location { get; set; }

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

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public class ReportsEmployeeAssetViewModel
{
    [Display(Name = "Employee ID")]
    public int EmployeeId { get; set; }

    [Display(Name = "Employee Code")]
    public string EmployeeCode { get; set; } = default!;

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = default!;

    [Display(Name = "Department")]
    public string? Department { get; set; }

    [Display(Name = "Position")]
    public string? Position { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }

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
    public string? Condition { get; set; }

    [Display(Name = "Assignment Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? AssignmentDate { get; set; }

    [Display(Name = "Expected Return")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? ExpectedReturnDate { get; set; }

    [Display(Name = "Last Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? LastMaintenanceDate { get; set; }

    [Display(Name = "Next Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? NextMaintenanceDate { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

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

    [Display(Name = "Condition")]
    public string? Condition { get; set; }

    [Display(Name = "Last Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? LastMaintenanceDate { get; set; }

    [Display(Name = "Next Maintenance")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? NextMaintenanceDate { get; set; }

    [Display(Name = "Maintenance Count")]
    public int MaintenanceCount { get; set; }

    [Display(Name = "Last Maintenance Notes")]
    public string? LastMaintenanceNotes { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = default!;
}

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

    [Display(Name = "Residual Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? ResidualValue { get; set; }

    [Display(Name = "Useful Life (Years)")]
    public int? UsefulLife { get; set; }

    [Display(Name = "Depreciation Start")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime? DepreciationStartDate { get; set; }

    [Display(Name = "Maintenance Count")]
    public int MaintenanceCount { get; set; }

    [Display(Name = "Total Maintenance Cost")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal? TotalMaintenanceCost { get; set; }
}

public class DashboardStatsViewModel
{
    public int TotalAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int UnderRepairAssets { get; set; }
    public int RetiredAssets { get; set; }
    public int ExpiredWarrantyCount { get; set; }
    public int UpcomingMaintenanceCount { get; set; }
    public decimal TotalAssetValue { get; set; }
    public int TotalEmployees { get; set; }
    public int PendingTransactions { get; set; }
    public int Last30DaysTransactions { get; set; }

    public decimal AssetUtilizationRate => TotalAssets > 0
        ? (decimal)(AvailableAssets + AssignedAssets) / TotalAssets * 100
        : 0;
}