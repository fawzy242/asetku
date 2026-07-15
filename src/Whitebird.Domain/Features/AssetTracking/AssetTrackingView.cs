namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Complete asset tracking detail view (header + timeline + software + photo + attachments)
/// </summary>
public class AssetTrackingView
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

    // Statistics
    public int TotalTransactions { get; set; }

    // Photo Profile
    public string? PhotoProfileUrl { get; set; }

    // Attachments
    public List<AssetTrackingAttachmentDto> Attachments { get; set; } = new();

    // Software Installation
    public AssetTrackingSoftwareInstallationDto? SoftwareInstallation { get; set; }

    // Timeline
    public List<AssetTimelineEntry> Timeline { get; set; } = new();
}

/// <summary>
/// DTO for software installation in asset tracking
/// </summary>
public class AssetTrackingSoftwareInstallationDto
{
    public int? OperatingSystem { get; set; }
    public string? OperatingSystemName { get; set; }
    public int? MicrosoftOffice { get; set; }
    public string? MicrosoftOfficeName { get; set; }
    public int? VisualStudio { get; set; }
    public string? VisualStudioName { get; set; }
    public int? DBMS { get; set; }
    public string? DBMSName { get; set; }
    public string? UserPassword { get; set; }
    public string? AdminOperationalPassword { get; set; }
    public bool ConnectToNAS { get; set; }
}

/// <summary>
/// DTO for attachment in asset tracking
/// </summary>
public class AssetTrackingAttachmentDto
{
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
}