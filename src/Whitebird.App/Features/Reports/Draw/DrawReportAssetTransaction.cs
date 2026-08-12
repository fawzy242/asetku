using Whitebird.App.Features.Common.Export;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportAssetTransaction : IDrawReport<ReportsAssetTransactionViewModel>
{
    private readonly IExportService<ReportsAssetTransactionViewModel> _exportService;

    public DrawReportAssetTransaction(IExportService<ReportsAssetTransactionViewModel> exportService)
    {
        _exportService = exportService;
    }

    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsAssetTransactionViewModel> data, string format)
    {
        var dataList = data.ToList();

        if (format.ToLower() == "pdf")
        {
            return await _exportService.ExportPdfAsync(dataList, "Asset Transaction Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return await _exportService.ExportExcelAsync(dataList, "Asset Transaction Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}