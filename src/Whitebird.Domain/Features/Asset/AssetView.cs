using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Asset;

public class AssetListViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetConditionName { get; set; }
    public string? OfficeName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public bool IsActive { get; set; }
}

public class AssetDetailViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? WarrantyPeriod { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? AssetCondition { get; set; }
    public string? AssetConditionName { get; set; }
    public int? AssetConditionPurchase { get; set; }
    public string? AssetConditionPurchaseName { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public bool? OperasionalOffice { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class AssetCreateViewModel
{
    [Required(ErrorMessage = "AssetCode is required")]
    [StringLength(50, ErrorMessage = "AssetCode cannot exceed 50 characters")]
    public string AssetCode { get; set; } = default!;

    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(100, ErrorMessage = "AssetName cannot exceed 100 characters")]
    public string AssetName { get; set; } = default!;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
    public string? Brand { get; set; }

    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
    public string? Model { get; set; }

    [StringLength(50, ErrorMessage = "SerialNumber cannot exceed 50 characters")]
    public string? SerialNumber { get; set; }

    [StringLength(50, ErrorMessage = "IMEI cannot exceed 50 characters")]
    public string? Imei { get; set; }

    [StringLength(50, ErrorMessage = "MacAddress cannot exceed 50 characters")]
    public string? MacAddress { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "PurchasePrice must be positive")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters")]
    public string? InvoiceNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "SupplierId must be greater than 0")]
    public int? SupplierId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "WarrantyPeriod must be positive")]
    public int? WarrantyPeriod { get; set; }

    public int? AssetCondition { get; set; }
    public int? AssetConditionPurchase { get; set; }

    public int? OfficeId { get; set; }

    [StringLength(50, ErrorMessage = "Hostname cannot exceed 50 characters")]
    public string? Hostname { get; set; }

    [StringLength(50, ErrorMessage = "IpAddress cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    public bool? OperasionalOffice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ResidualValue must be positive")]
    public decimal? ResidualValue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "UsefulLife must be greater than 0")]
    public int? UsefulLife { get; set; }

    public DateTime? DepreciationStartDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

public class AssetUpdateViewModel
{
    [Required(ErrorMessage = "AssetCode is required")]
    [StringLength(50, ErrorMessage = "AssetCode cannot exceed 50 characters")]
    public string AssetCode { get; set; } = default!;

    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(100, ErrorMessage = "AssetName cannot exceed 100 characters")]
    public string AssetName { get; set; } = default!;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
    public string? Brand { get; set; }

    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
    public string? Model { get; set; }

    [StringLength(50, ErrorMessage = "SerialNumber cannot exceed 50 characters")]
    public string? SerialNumber { get; set; }

    [StringLength(50, ErrorMessage = "IMEI cannot exceed 50 characters")]
    public string? Imei { get; set; }

    [StringLength(50, ErrorMessage = "MacAddress cannot exceed 50 characters")]
    public string? MacAddress { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "PurchasePrice must be positive")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters")]
    public string? InvoiceNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "SupplierId must be greater than 0")]
    public int? SupplierId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "WarrantyPeriod must be positive")]
    public int? WarrantyPeriod { get; set; }

    public int? AssetCondition { get; set; }
    public int? AssetConditionPurchase { get; set; }

    public int? OfficeId { get; set; }

    [StringLength(50, ErrorMessage = "Hostname cannot exceed 50 characters")]
    public string? Hostname { get; set; }

    [StringLength(50, ErrorMessage = "IpAddress cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    public bool? OperasionalOffice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ResidualValue must be positive")]
    public decimal? ResidualValue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "UsefulLife must be greater than 0")]
    public int? UsefulLife { get; set; }

    public DateTime? DepreciationStartDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
}

public class AssetTrackingViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CurrentStatus { get; set; } = default!;
    public string? CategoryName { get; set; }
    public string? CurrentHolderName { get; set; }
    public string? CurrentLocation { get; set; }
    public string? Condition { get; set; }
    public bool IsOnLoan { get; set; }
    public bool IsInMaintenance { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? LoanDueDate { get; set; }
    public int TotalTransactions { get; set; }
    public List<AssetTimelineEntry> Timeline { get; set; } = new();
}

public class AssetTimelineEntry
{
    public DateTime Date { get; set; }
    public string ActivityType { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PreviousHolder { get; set; }
    public string? NewHolder { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Notes { get; set; }
}

public class AssetCurrentStatusDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CurrentStatus { get; set; } = default!;
    public int? CurrentStatusType { get; set; }
    public string? CurrentHolderName { get; set; }
    public int? CurrentHolderId { get; set; }
    public string? CurrentOfficeName { get; set; }
    public int? CurrentOfficeId { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? ConditionName { get; set; }
    public int? ConditionCode { get; set; }
}