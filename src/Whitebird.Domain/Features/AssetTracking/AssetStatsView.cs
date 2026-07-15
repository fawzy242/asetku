namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Asset header stats view (without timeline)
/// </summary>
public class AssetStatsView
{
    // Asset Info
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }

    // Current Status
    public string CurrentStatus { get; set; } = string.Empty;
    public string? CurrentHolderName { get; set; }
    public string? CurrentLocation { get; set; }
    public string? ConditionName { get; set; }

    // Flags
    public bool IsOnLoan { get; set; }
    public bool IsInMaintenance { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? LoanDueDate { get; set; }

    // Purchase Info
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    // Photo Profile
    public string? PhotoProfileUrl { get; set; }

    // Attachments
    public List<AssetTrackingAttachmentDto> Attachments { get; set; } = new();

    // Software Installation
    public AssetTrackingSoftwareInstallationDto? SoftwareInstallation { get; set; }
}