using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Reports;
using Whitebird.Infra.Features.Reports;

namespace Whitebird.App.Features.Reports;

/// <summary>
/// Service implementation for Reports business logic
/// </summary>
public class ReportsService : BaseService, IReportsService
{
    private readonly IReportsReps _repository;
    private readonly IMasterDataLookupService _masterDataLookupService;

    public ReportsService(
        IReportsReps repository,
        IMasterDataLookupService masterDataLookupService,
        ILogger<ReportsService> logger) : base(logger)
    {
        _repository = repository;
        _masterDataLookupService = masterDataLookupService;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetTransactionReportsAsync(startDate, endDate, transactionType);
            return ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>.Success(data);
        }, "get asset transaction reports");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(
        string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetInventoryReportsAsync(status, categoryId, supplierId);
            return ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>.Success(data);
        }, "get asset inventory reports");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(
        int? employeeId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetEmployeeAssetReportsAsync(employeeId);
            return ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>.Success(data);
        }, "get employee asset reports");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetMaintenanceReportsAsync(startDate, endDate, isUpcoming);
            return ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>.Success(data);
        }, "get maintenance reports");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetFinancialReportsAsync(startDate, endDate);
            return ServiceResult<IEnumerable<ReportsFinancialViewModel>>.Success(data);
        }, "get financial reports");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var stats = await _repository.GetDashboardStatsAsync();
            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<MonthlyStatDto>>> GetMonthlyStatsAsync(int year)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetMonthlyStatsAsync(year);
            return ServiceResult<IEnumerable<MonthlyStatDto>>.Success(data);
        }, "get monthly stats");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetCategoryBreakdownAsync();
            return ServiceResult<IEnumerable<CategoryBreakdownDto>>.Success(data);
        }, "get category breakdown");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<RecentTransactionDto>>> GetRecentTransactionsAsync(int limit = 10)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetRecentTransactionsAsync(limit);
            return ServiceResult<IEnumerable<RecentTransactionDto>>.Success(data);
        }, "get recent transactions");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.ExportAssetTransactionReportsAsync(startDate, endDate, transactionType);
            var title = "Asset Transaction Report";
            var subtitle = GetSubtitle(startDate, endDate, transactionType);
            return GenerateExcel(data, title, subtitle);
        }, "generate asset transaction excel");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(
        string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.ExportAssetInventoryReportsAsync(status, categoryId, supplierId);
            var title = "Asset Inventory Report";
            var subtitle = GetSubtitle(status, categoryId, supplierId);
            return GenerateExcel(data, title, subtitle);
        }, "generate asset inventory excel");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(
        int? employeeId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.ExportEmployeeAssetReportsAsync(employeeId);
            var title = "Employee Asset Report";
            var subtitle = GetSubtitle(employeeId);
            return GenerateExcel(data, title, subtitle);
        }, "generate employee asset excel");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.ExportMaintenanceReportsAsync(startDate, endDate, isUpcoming);
            var title = isUpcoming == true ? "Upcoming Maintenance Report" : "Maintenance History Report";
            var subtitle = GetSubtitle(startDate, endDate, isUpcoming);
            return GenerateExcel(data, title, subtitle);
        }, "generate maintenance excel");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.ExportFinancialReportsAsync(startDate, endDate);
            var title = "Financial Report";
            var subtitle = GetSubtitle(startDate, endDate);
            return GenerateExcel(data, title, subtitle);
        }, "generate financial excel");
    }

    #region Private Helpers

    private ServiceResult<byte[]> GenerateExcel<T>(IEnumerable<T> data, string title, string subtitle)
    {
        if (data == null || !data.Any())
        {
            using var EmptyPackage = new ExcelPackage();
            var EmptyWorksheet = EmptyPackage.Workbook.Worksheets.Add(title.Length > 31 ? title.Substring(0, 31) : title);
            EmptyWorksheet.Cells["A1"].Value = "No data found for the selected filters";
            EmptyWorksheet.Cells["A1"].Style.Font.Bold = true;
            EmptyWorksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            return ServiceResult<byte[]>.Success(EmptyPackage.GetAsByteArray());
        }

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(title.Length > 31 ? title.Substring(0, 31) : title);

        var titleCell = worksheet.Cells["A1:X1"];
        titleCell.Merge = true;
        titleCell.Value = title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.Size = 16;
        titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        var dateCell = worksheet.Cells["A2:X2"];
        dateCell.Merge = true;
        dateCell.Value = $"{subtitle} | Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        dateCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        dateCell.Style.Font.Italic = true;
        dateCell.Style.Font.Size = 10;

        AddHeaders(worksheet, 4, typeof(T));
        AddData(worksheet, data, 5);

        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var dataRange = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                             worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        return ServiceResult<byte[]>.Success(package.GetAsByteArray());
    }

    private void AddHeaders(ExcelWorksheet worksheet, int startRow, Type type)
    {
        var properties = type.GetProperties();
        int col = 1;

        foreach (var property in properties)
        {
            var displayAttr = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ??
                             property.GetCustomAttribute<DisplayAttribute>()?.Name;
            var headerName = displayAttr ?? property.Name;

            var cell = worksheet.Cells[startRow, col];
            cell.Value = headerName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            col++;
        }
    }

    private void AddData<T>(ExcelWorksheet worksheet, IEnumerable<T> data, int startRow)
    {
        var properties = typeof(T).GetProperties();
        int row = startRow;

        foreach (var item in data)
        {
            int col = 1;
            foreach (var property in properties)
            {
                var value = property.GetValue(item);
                var cell = worksheet.Cells[row, col];

                if (value == null)
                {
                    cell.Value = string.Empty;
                }
                else if (value is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.Numberformat.Format = "yyyy-MM-dd";
                }
                else if (value is DateTimeOffset dto)
                {
                    cell.Value = dto.DateTime;
                    cell.Style.Numberformat.Format = "yyyy-MM-dd";
                }
                else if (value is decimal dec)
                {
                    cell.Value = dec;
                    cell.Style.Numberformat.Format = "#,##0.00";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (value is int || value is long || value is short)
                {
                    cell.Value = value;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (value is bool b)
                {
                    cell.Value = b ? "Yes" : "No";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else
                {
                    cell.Value = value.ToString();
                }
                col++;
            }
            row++;
        }
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate, string? transactionType = null)
    {
        var parts = new List<string>();
        if (startDate.HasValue && endDate.HasValue)
            parts.Add($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        else if (startDate.HasValue)
            parts.Add($"From: {startDate:yyyy-MM-dd}");
        else if (endDate.HasValue)
            parts.Add($"Until: {endDate:yyyy-MM-dd}");

        if (!string.IsNullOrEmpty(transactionType))
            parts.Add($"Type: {transactionType}");

        return parts.Any() ? string.Join(" | ", parts) : "All Records";
    }

    private string GetSubtitle(string? status, int? categoryId, int? supplierId)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(status)) parts.Add($"Status: {status}");
        if (categoryId.HasValue) parts.Add($"Category ID: {categoryId}");
        if (supplierId.HasValue) parts.Add($"Supplier ID: {supplierId}");
        return parts.Any() ? string.Join(" | ", parts) : "All Assets";
    }

    private string GetSubtitle(int? employeeId)
    {
        return employeeId.HasValue ? $"Employee ID: {employeeId}" : "All Employees";
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate, bool? isUpcoming)
    {
        var parts = new List<string>();
        if (startDate.HasValue && endDate.HasValue)
            parts.Add($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        if (isUpcoming == true) parts.Add("Upcoming Only");
        return parts.Any() ? string.Join(" | ", parts) : "All Maintenance Records";
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
            return $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
        if (startDate.HasValue)
            return $"From: {startDate:yyyy-MM-dd}";
        if (endDate.HasValue)
            return $"Until: {endDate:yyyy-MM-dd}";
        return "All Records";
    }

    #endregion
}