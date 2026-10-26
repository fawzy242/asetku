using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Whitebird.App.Features.Common.Export;

/// <summary>
/// Base abstract class for export services
/// </summary>
/// <typeparam name="T">The type of data to export</typeparam>
public class ExportService<T> : IExportService<T>
{
    public ExportService()
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Gets the display name for a property
    /// </summary>
    protected virtual string GetDisplayName(PropertyInfo property)
    {
        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr != null) return displayNameAttr.DisplayName;

        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
            return displayAttr.Name;

        return property.Name;
    }

    /// <summary>
    /// Gets the properties to export (can be overridden to filter)
    /// </summary>
    protected virtual IEnumerable<PropertyInfo> GetExportProperties()
    {
        return typeof(T).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(DisplayAttribute), true).Any() ||
                        p.GetCustomAttributes(typeof(DisplayNameAttribute), true).Any());
    }

    /// <inheritdoc />
    public virtual async Task<byte[]> ExportExcelAsync(IEnumerable<T> data, string title, string? subtitle = null)
    {
        var dataList = data.ToList();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(title.Length > 31 ? title.Substring(0, 31) : title);

        var properties = GetExportProperties().ToList();
        int colCount = properties.Count;

        // Title
        var titleCell = worksheet.Cells[1, 1, 1, colCount];
        titleCell.Merge = true;
        titleCell.Value = title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.Size = 16;
        titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Row(1).Height = 30;

        // Subtitle
        if (!string.IsNullOrEmpty(subtitle))
        {
            var subtitleCell = worksheet.Cells[2, 1, 2, colCount];
            subtitleCell.Merge = true;
            subtitleCell.Value = subtitle;
            subtitleCell.Style.Font.Size = 10;
            subtitleCell.Style.Font.Italic = true;
            subtitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(2).Height = 25;
        }

        // Headers
        int headerRow = string.IsNullOrEmpty(subtitle) ? 3 : 4;
        for (int i = 0; i < properties.Count; i++)
        {
            var header = GetDisplayName(properties[i]);
            var cell = worksheet.Cells[headerRow, i + 1];
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Data
        int dataRow = headerRow + 1;
        foreach (var item in dataList)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var value = properties[i].GetValue(item);
                var cell = worksheet.Cells[dataRow, i + 1];
                cell.Value = value ?? string.Empty;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Format based on type
                if (value is DateTime dt)
                {
                    cell.Style.Numberformat.Format = "yyyy-MM-dd";
                }
                else if (value is decimal dec)
                {
                    cell.Style.Numberformat.Format = "#,##0.00";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (value is int || value is long)
                {
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (value is bool b)
                {
                    cell.Value = b ? "Yes" : "No";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }
            dataRow++;
        }

        // Auto fit
        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        return await Task.FromResult(package.GetAsByteArray());
    }

    /// <inheritdoc />
    public virtual Task<byte[]> ExportPdfAsync(IEnumerable<T> data, string title, string? subtitle = null)
    {
        // PDF export is not implemented in this project (no PDF library is present in
        // Directory.Packages.props). The previous implementation returned Excel bytes
        // here, which callers then served with Content-Type application/pdf and a .pdf
        // file name — producing corrupt downloads. Fail loudly so callers reject the
        // request instead of serving an invalid file.
        throw new NotSupportedException(
            "PDF export is not implemented. Use format=excel. To enable PDF, add a PDF-generation " +
            "library to Directory.Packages.props and implement ExportPdfAsync.");
    }
}