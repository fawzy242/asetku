namespace Whitebird.Domain.Features.Asset.View;

public class DashboardStatsViewModel
{
    public int TotalAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int AssetsInMaintenance { get; set; }
    public int UnderRepairAssets { get; set; }
    public int DamagedAssets { get; set; }
    public int RetiredAssets { get; set; }
    public int ExpiredWarrantyCount { get; set; }
    public int UpcomingMaintenanceCount { get; set; }
    public int OverdueLoanCount { get; set; }
    public decimal TotalAssetValue { get; set; }

    public decimal AssetUtilizationRate => TotalAssets > 0
        ? (decimal)(AvailableAssets + AssignedAssets + AssetsOnLoan) / TotalAssets * 100
        : 0;
}