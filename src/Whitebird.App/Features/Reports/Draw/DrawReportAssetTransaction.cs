using OfficeOpenXml;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportAssetTransaction : IDrawReport<ReportsAssetTransactionViewModel>
{
    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsAssetTransactionViewModel> data, string format)
    {
        if (format.ToLower() == "pdf")
        {
            // For PDF, we'll implement later or use a library
            // For now, fallback to Excel
            return await GenerateExcelAsync(data);
        }

        return await GenerateExcelAsync(data);
    }

    private async Task<byte[]> GenerateExcelAsync(IEnumerable<ReportsAssetTransactionViewModel> data)
    {
        var dataList = data.ToList();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Asset Transaction");

        var properties = typeof(ReportsAssetTransactionViewModel).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(DisplayAttribute), true).Any() ||
                        p.GetCustomAttributes(typeof(DisplayNameAttribute), true).Any())
            .ToList();

        int colCount = properties.Count;

        // Title
        DrawReportHelper.AddTitle(worksheet, 1, "Asset Transaction Report", colCount);
        DrawReportHelper.AddSubtitle(worksheet, 2, $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", colCount);

        // Headers
        int row = 4;
        for (int i = 0; i < properties.Count; i++)
        {
            var header = DrawReportHelper.GetDisplayName(properties[i]);
            DrawReportHelper.ApplyHeaderStyle(worksheet, row, i + 1, header);
        }

        // Data
        int dataRow = row + 1;
        foreach (var item in dataList)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var value = properties[i].GetValue(item);
                DrawReportHelper.ApplyDataCellStyle(worksheet, dataRow, i + 1, value);
            }
            dataRow++;
        }

        DrawReportHelper.AutoFitColumns(worksheet);

        return await Task.FromResult(package.GetAsByteArray());
    }
}