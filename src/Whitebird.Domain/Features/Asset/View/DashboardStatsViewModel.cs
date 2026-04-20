namespace Whitebird.Domain.Features.Asset.View;

public class DashboardStatsViewModel
{
    public int TotalAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int UnderRepairAssets { get; set; }
    public int RetiredAssets { get; set; }
    public int ExpiredWarrantyCount { get; set; }
    public int UpcomingMaintenanceCount { get; set; }
    public decimal TotalAssetValue { get; set; }

    public decimal AssetUtilizationRate => TotalAssets > 0
        ? (decimal)(AvailableAssets + AssignedAssets) / TotalAssets * 100
        : 0;
}