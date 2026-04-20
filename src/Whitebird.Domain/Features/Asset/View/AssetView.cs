using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Asset.View;

public class AssetListViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CategoryName { get; set; } = "Unknown";
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string Status { get; set; } = default!;
    public string? CurrentHolderName { get; set; }
    public string? Condition { get; set; }
    public string? Location { get; set; }
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
    public string CategoryName { get; set; } = "Unknown";
    public string? SubCategory { get; set; }
    public string? AssetType { get; set; }
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
    public string? Condition { get; set; }
    public string Status { get; set; } = default!;
    public string? Location { get; set; }
    public int? CurrentHolderId { get; set; }
    public string? CurrentHolderName { get; set; }
    public int? ResponsiblePartyId { get; set; }
    public string? ResponsiblePartyName { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class AssetCreateViewModel
{
    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(100, ErrorMessage = "AssetName cannot exceed 100 characters")]
    public string AssetName { get; set; } = default!;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    public string? SubCategory { get; set; }
    public string? AssetType { get; set; }

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

    [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
    public string? Condition { get; set; } = "Good";

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    public int? CurrentHolderId { get; set; }
    public int? ResponsiblePartyId { get; set; }

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
    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(100, ErrorMessage = "AssetName cannot exceed 100 characters")]
    public string AssetName { get; set; } = default!;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    public string? SubCategory { get; set; }
    public string? AssetType { get; set; }

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

    [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
    public string? Condition { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string Status { get; set; } = default!;

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    public int? CurrentHolderId { get; set; }
    public int? ResponsiblePartyId { get; set; }

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