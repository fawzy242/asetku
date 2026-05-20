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
        
        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            var columnName = hasHeader 
                ? worksheet.Cells[headerRow, col].Text?.Trim() ?? $"Column{col}"
                : $"Column{col}";
            
            if (dataTable.Columns.Contains(columnName))
                columnName = $"{columnName}_{col}";
            
            dataTable.Columns.Add(columnName);
        }
        
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
    
    public static async Task<DataTable> ReadTxtToDataTableAsync(Stream fileStream, char delimiter = '\t', bool hasHeader = true)
    {
        var dataTable = new DataTable();
        
        using var reader = new StreamReader(fileStream);
        var lines = new List<string>();
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);
        }
        
        if (lines.Count == 0)
            throw new InvalidOperationException("TXT file is empty");
        
        var startRow = hasHeader ? 1 : 0;
        
        if (hasHeader && lines.Count > 0)
        {
            var headers = lines[0].Split(delimiter);
            foreach (var header in headers)
            {
                var columnName = header.Trim();
                if (dataTable.Columns.Contains(columnName))
                    columnName = $"{columnName}_{Guid.NewGuid():N}";
                dataTable.Columns.Add(columnName);
            }
        }
        else
        {
            var firstLineParts = lines[0].Split(delimiter);
            for (int i = 0; i < firstLineParts.Length; i++)
            {
                dataTable.Columns.Add($"Column{i + 1}");
            }
        }
        
        for (int i = startRow; i < lines.Count; i++)
        {
            var parts = lines[i].Split(delimiter);
            var dataRow = dataTable.NewRow();
            
            for (int j = 0; j < parts.Length && j < dataTable.Columns.Count; j++)
            {
                dataRow[j] = parts[j].Trim();
            }
            
            dataTable.Rows.Add(dataRow);
        }
        
        return dataTable;
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
    
    public static byte[] GenerateTxtTemplate(string fileName, Dictionary<string, string> columns, char delimiter = '\t', string? description = null)
    {
        var lines = new List<string>();
        
        if (!string.IsNullOrEmpty(description))
        {
            lines.Add($"# {description}");
            lines.Add($"# Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            lines.Add("");
        }
        
        var headerLine = string.Join(delimiter.ToString(), columns.Keys);
        lines.Add(headerLine);
        
        var exampleLine = string.Join(delimiter.ToString(), columns.Select(c => $"[{c.Value}]"));
        lines.Add(exampleLine);
        
        var content = string.Join(Environment.NewLine, lines);
        return System.Text.Encoding.UTF8.GetBytes(content);
    }
}