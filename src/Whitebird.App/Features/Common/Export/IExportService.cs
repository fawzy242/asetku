namespace Whitebird.App.Features.Common.Export;

/// <summary>
/// Generic interface for export service
/// </summary>
/// <typeparam name="T">The type of data to export</typeparam>
public interface IExportService<T>
{
    /// <summary>
    /// Exports data to Excel format
    /// </summary>
    /// <param name="data">The data to export</param>
    /// <param name="title">The title of the report</param>
    /// <param name="subtitle">Optional subtitle</param>
    /// <returns>Excel file bytes</returns>
    Task<byte[]> ExportExcelAsync(IEnumerable<T> data, string title, string? subtitle = null);

    /// <summary>
    /// Exports data to PDF format
    /// </summary>
    /// <param name="data">The data to export</param>
    /// <param name="title">The title of the report</param>
    /// <param name="subtitle">Optional subtitle</param>
    /// <returns>PDF file bytes</returns>
    Task<byte[]> ExportPdfAsync(IEnumerable<T> data, string title, string? subtitle = null);
}