namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// DTO for asset lookup/dropdown in Asset Tracking menu
/// </summary>
public class AssetTrackingGetDataDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}