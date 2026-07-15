using OfficeOpenXml;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports.Draw;

public class DrawReportAssetInventory : IDrawReport<ReportsAssetInventoryViewModel>
{
    public async Task<byte[]> GenerateAsync(IEnumerable<ReportsAssetInventoryViewModel> data, string format)
    {
        if (format.ToLower() == "pdf")
        {
            return await GenerateExcelAsync(data);
        }
        return await GenerateExcelAsync(data);
    }

    private async Task<byte[]> GenerateExcelAsync(IEnumerable<ReportsAssetInventoryViewModel> data)
    {
        var dataList = data.ToList();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Asset Inventory");

        var properties = typeof(ReportsAssetInventoryViewModel).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(DisplayAttribute), true).Any() ||
                        p.GetCustomAttributes(typeof(DisplayNameAttribute), true).Any())
            .ToList();

        int colCount = properties.Count;

        DrawReportHelper.AddTitle(worksheet, 1, "Asset Inventory Report", colCount);
        DrawReportHelper.AddSubtitle(worksheet, 2, $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", colCount);

        int row = 4;
        for (int i = 0; i < properties.Count; i++)
        {
            var header = DrawReportHelper.GetDisplayName(properties[i]);
            DrawReportHelper.ApplyHeaderStyle(worksheet, row, i + 1, header);
        }

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