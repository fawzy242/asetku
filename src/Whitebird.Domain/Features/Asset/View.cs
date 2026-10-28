namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// View model untuk Asset grid/list (bukan Entity!)
/// Data dari JOIN: Category, Supplier, MasterData (AssetConditionPurchase)
/// CurrentStatus dihitung dari active transaction
/// </summary>
public class AssetListView
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
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
    public string? CurrentStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>
/// View model for asset detail (response from GetById)
/// </summary>
public class AssetDetailView
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
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
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Software Installation
    public SoftwareInstallationView? SoftwareInstallation { get; set; }

    // ============================================================
    // Photo Profile & Attachments
    // ============================================================
    public string? PhotoProfileUrl { get; set; }
    public List<AssetAttachmentViewDto> Attachments { get; set; } = new();
}

/// <summary>
/// DTO for asset attachment in detail view
/// </summary>
public class AssetAttachmentViewDto
{
    public int AssetAttachmentId { get; set; }
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// View model untuk asset dropdown (minimal data)
/// </summary>
public class AssetDropdownView
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}

/// <summary>
/// View model for Software Installation detail
/// </summary>
public class SoftwareInstallationView
{
    public int SoftwareInstallationId { get; set; }
    public int AssetId { get; set; }
    public string? UserPassword { get; set; }
    public string? AdminOperationalPassword { get; set; }
    public int? OperatingSystem { get; set; }
    public string? OperatingSystemName { get; set; }
    public int? MicrosoftOffice { get; set; }
    public string? MicrosoftOfficeName { get; set; }
    public int? VisualStudio { get; set; }
    public string? VisualStudioName { get; set; }
    public int? DBMS { get; set; }
    public string? DBMSName { get; set; }
    public bool ConnectToNAS { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}