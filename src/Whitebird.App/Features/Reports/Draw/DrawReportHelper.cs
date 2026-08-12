using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Whitebird.App.Features.Reports.Draw;

/// <summary>
/// Helper class for generating reports (Excel styling, formatting)
/// </summary>
public static class DrawReportHelper
{
    static DrawReportHelper()
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Get display name from DisplayNameAttribute or DisplayAttribute
    /// </summary>
    public static string GetDisplayName(PropertyInfo property)
    {
        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr != null) return displayNameAttr.DisplayName;

        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
            return displayAttr.Name;

        return property.Name;
    }

    /// <summary>
    /// Apply Excel header styling
    /// </summary>
    public static void ApplyHeaderStyle(ExcelWorksheet worksheet, int row, int col, string header)
    {
        var cell = worksheet.Cells[row, col];
        cell.Value = header;
        cell.Style.Font.Bold = true;
        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
    }

    /// <summary>
    /// Apply Excel data cell styling with border
    /// </summary>
    public static void ApplyDataCellStyle(ExcelWorksheet worksheet, int row, int col, object? value)
    {
        var cell = worksheet.Cells[row, col];
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

    /// <summary>
    /// Add title to worksheet
    /// </summary>
    public static void AddTitle(ExcelWorksheet worksheet, int row, string title, int columnCount)
    {
        var cell = worksheet.Cells[row, 1, row, columnCount];
        cell.Merge = true;
        cell.Value = title;
        cell.Style.Font.Bold = true;
        cell.Style.Font.Size = 16;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Row(row).Height = 30;
    }

    /// <summary>
    /// Add subtitle/date to worksheet
    /// </summary>
    public static void AddSubtitle(ExcelWorksheet worksheet, int row, string subtitle, int columnCount)
    {
        var cell = worksheet.Cells[row, 1, row, columnCount];
        cell.Merge = true;
        cell.Value = subtitle;
        cell.Style.Font.Size = 10;
        cell.Style.Font.Italic = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Row(row).Height = 25;
    }

    /// <summary>
    /// Auto fit columns
    /// </summary>
    public static void AutoFitColumns(ExcelWorksheet worksheet)
    {
        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }
}