using Whitebird.App.Features.Common.Export;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportAssetInventory : IDrawReport<ReportsAssetInventoryViewModel>
{
    private readonly IExportService<ReportsAssetInventoryViewModel> _exportService;

    public DrawReportAssetInventory(IExportService<ReportsAssetInventoryViewModel> exportService)
    {
        _exportService = exportService;
    }

    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsAssetInventoryViewModel> data, string format)
    {
        var dataList = data.ToList();

        if (format.ToLower() == "pdf")
        {
            return await _exportService.ExportPdfAsync(dataList, "Asset Inventory Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return await _exportService.ExportExcelAsync(dataList, "Asset Inventory Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}