using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// Grid request for Asset
/// </summary>
public class AssetGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by status (Available, Assigned, On Loan, In Maintenance, Damaged, Active, Inactive)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by CategoryId
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Filter by OfficeId
    /// </summary>
    public int? OfficeId { get; set; }

    /// <summary>
    /// Filter by SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
}

public class AssetListViewModel
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetConditionPurchaseName { get; set; }
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
    public int? AssetConditionPurchase { get; set; }
    public string? AssetConditionPurchaseName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Software Installation
    public SoftwareInstallationView? SoftwareInstallation { get; set; }
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

    public int? AssetConditionPurchase { get; set; }

    [StringLength(50, ErrorMessage = "Hostname cannot exceed 50 characters")]
    public string? Hostname { get; set; }

    [StringLength(50, ErrorMessage = "IpAddress cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    // Software Installation
    public SoftwareInstallationDto? SoftwareInstallation { get; set; }
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

    public int? AssetConditionPurchase { get; set; }

    [StringLength(50, ErrorMessage = "Hostname cannot exceed 50 characters")]
    public string? Hostname { get; set; }

    [StringLength(50, ErrorMessage = "IpAddress cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }

    // Software Installation
    public SoftwareInstallationDto? SoftwareInstallation { get; set; }
}

/// <summary>
/// DTO for Software Installation Create/Update
/// </summary>
public class SoftwareInstallationDto
{
    [StringLength(50, ErrorMessage = "UserPassword cannot exceed 50 characters")]
    public string? UserPassword { get; set; }

    [StringLength(50, ErrorMessage = "AdminOperationalPassword cannot exceed 50 characters")]
    public string? AdminOperationalPassword { get; set; }

    public int? OperatingSystem { get; set; }
    public int? MicrosoftOffice { get; set; }
    public int? VisualStudio { get; set; }
    public int? DBMS { get; set; }
    public bool ConnectToNAS { get; set; }
}