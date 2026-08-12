using Whitebird.App.Features.Common.Export;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportFinancial : IDrawReport<ReportsFinancialViewModel>
{
    private readonly IExportService<ReportsFinancialViewModel> _exportService;

    public DrawReportFinancial(IExportService<ReportsFinancialViewModel> exportService)
    {
        _exportService = exportService;
    }

    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsFinancialViewModel> data, string format)
    {
        var dataList = data.ToList();

        if (format.ToLower() == "pdf")
        {
            return await _exportService.ExportPdfAsync(dataList, "Financial Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return await _exportService.ExportExcelAsync(dataList, "Financial Report", $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}