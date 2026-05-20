using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;

namespace Whitebird.App.Features.Common.Import;

public static class ExcelHelper
{
    static ExcelHelper()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static async Task<DataTable> ReadExcelToDataTableAsync(Stream fileStream, bool hasHeader = true)
    {
        var dataTable = new DataTable();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet.Dimension == null)
            throw new InvalidOperationException("Excel file is empty");

        var startRow = hasHeader ? 1 : 0;
        var headerRow = hasHeader ? 1 : 0;

        // Add columns
        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            var columnName = hasHeader
                ? worksheet.Cells[headerRow, col].Text?.Trim() ?? $"Column{col}"
                : $"Column{col}";

            if (dataTable.Columns.Contains(columnName))
                columnName = $"{columnName}_{col}";

            dataTable.Columns.Add(columnName);
        }

        // Add data rows
        for (int row = startRow + 1; row <= worksheet.Dimension.Rows; row++)
        {
            var dataRow = dataTable.NewRow();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var cell = worksheet.Cells[row, col];
                dataRow[col - 1] = cell.Text ?? string.Empty;
            }
            dataTable.Rows.Add(dataRow);
        }

        return await Task.FromResult(dataTable);
    }

    public static byte[] GenerateTemplate(string sheetName, Dictionary<string, string> columns, string? description = null)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        if (!string.IsNullOrEmpty(description))
        {
            worksheet.Cells[1, 1].Value = description;
            worksheet.Cells[1, 1, 1, columns.Count].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 12;
            worksheet.Row(1).Height = 25;
        }

        var startRow = string.IsNullOrEmpty(description) ? 1 : 2;

        int col = 1;
        foreach (var column in columns)
        {
            worksheet.Cells[startRow, col].Value = column.Key;
            worksheet.Cells[startRow, col].Style.Font.Bold = true;
            worksheet.Cells[startRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            if (!string.IsNullOrEmpty(column.Value))
            {
                worksheet.Cells[startRow, col].AddComment(column.Value, "Instruction");
            }
            col++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }
}