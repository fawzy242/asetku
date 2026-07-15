namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// DTO for asset list in dropdown
/// </summary>
public class AssetTransactionAssetDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
}