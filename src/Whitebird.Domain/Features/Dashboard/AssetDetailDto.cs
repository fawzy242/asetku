namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// DTO for asset detail in drilldown grid
/// </summary>
public class AssetDetailDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? CurrentStatus { get; set; }
    public string? CurrentHolderName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public bool IsActive { get; set; }
}