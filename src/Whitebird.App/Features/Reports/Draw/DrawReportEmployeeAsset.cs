using Whitebird.App.Features.Common.Export;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportEmployeeAsset : IDrawReport<ReportsEmployeeAssetViewModel>
{
    private readonly IExportService<ReportsEmployeeAssetViewModel> _exportService;

    public DrawReportEmployeeAsset(IExportService<ReportsEmployeeAssetViewModel> exportService)
    {
        _exportService = exportService;
    }

    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsEmployeeAssetViewModel> data, string format)
    {
        var dataList = data.ToList();

        if (format.ToLower() == "pdf")
        {
            return await _exportService.ExportPdfAsync(dataList, "Employee Asset Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return await _exportService.ExportExcelAsync(dataList, "Employee Asset Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}