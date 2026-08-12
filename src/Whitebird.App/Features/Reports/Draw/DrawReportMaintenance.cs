using Whitebird.App.Features.Common.Export;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportMaintenance : IDrawReport<ReportsMaintenanceViewModel>
{
    private readonly IExportService<ReportsMaintenanceViewModel> _exportService;

    public DrawReportMaintenance(IExportService<ReportsMaintenanceViewModel> exportService)
    {
        _exportService = exportService;
    }

    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsMaintenanceViewModel> data, string format)
    {
        var dataList = data.ToList();

        if (format.ToLower() == "pdf")
        {
            return await _exportService.ExportPdfAsync(dataList, "Maintenance Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return await _exportService.ExportExcelAsync(dataList, "Maintenance Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}