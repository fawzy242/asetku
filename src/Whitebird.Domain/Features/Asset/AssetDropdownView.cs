namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// View model untuk asset dropdown (minimal data)
/// </summary>
public class AssetDropdownView
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}