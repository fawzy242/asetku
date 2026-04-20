using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Whitebird.App.Features.Common.Service;
using Whitebird.App.Features.Reports.Interfaces;
using Whitebird.Domain.Features.Reports.View;
using Whitebird.Infra.Features.Reports;

namespace Whitebird.App.Features.Reports.Service;

public class ReportsService : BaseService, IReportsService
{
    private readonly IReportsReps _repository;

    public ReportsService(IReportsReps repository, ILogger<ReportsService> logger) : base(logger)
    {
        _repository = repository;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    public async Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetTransactionReportsAsync(startDate, endDate, transactionType);
            return ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>.Success(data);
        }, "get asset transaction reports");
    }

    public async Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetInventoryReportsAsync(status, categoryId, supplierId);
            return ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>.Success(data);
        }, "get asset inventory reports");
    }

    public async Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetEmployeeAssetReportsAsync(employeeId, department);
            return ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>.Success(data);
        }, "get employee asset reports");
    }

    public async Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetMaintenanceReportsAsync(startDate, endDate, isUpcoming);
            return ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>.Success(data);
        }, "get maintenance reports");
    }

    public async Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetFinancialReportsAsync(startDate, endDate);
            return ServiceResult<IEnumerable<ReportsFinancialViewModel>>.Success(data);
        }, "get financial reports");
    }

    public async Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var stats = await _repository.GetDashboardStatsAsync();
            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    public async Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetTransactionReportsAsync(startDate, endDate, transactionType);
            var title = "Asset Transaction Report";
            var subtitle = GetSubtitle(startDate, endDate, transactionType);
            return GenerateExcel(data, title, subtitle);
        }, "generate asset transaction excel");
    }

    public async Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetAssetInventoryReportsAsync(status, categoryId, supplierId);
            var title = "Asset Inventory Report";
            var subtitle = GetSubtitle(status, categoryId, supplierId);
            return GenerateExcel(data, title, subtitle);
        }, "generate asset inventory excel");
    }

    public async Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(int? employeeId = null, string? department = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetEmployeeAssetReportsAsync(employeeId, department);
            var title = "Employee Asset Report";
            var subtitle = GetSubtitle(employeeId, department);
            return GenerateExcel(data, title, subtitle);
        }, "generate employee asset excel");
    }

    public async Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetMaintenanceReportsAsync(startDate, endDate, isUpcoming);
            var title = isUpcoming == true ? "Upcoming Maintenance Report" : "Maintenance History Report";
            var subtitle = GetSubtitle(startDate, endDate, isUpcoming);
            return GenerateExcel(data, title, subtitle);
        }, "generate maintenance excel");
    }

    public async Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _repository.GetFinancialReportsAsync(startDate, endDate);
            var title = "Financial Report";
            var subtitle = GetSubtitle(startDate, endDate);
            return GenerateExcel(data, title, subtitle);
        }, "generate financial excel");
    }

    private ServiceResult<byte[]> GenerateExcel<T>(IEnumerable<T> data, string title, string subtitle)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(title);

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

        AddHeaders(worksheet, 3, typeof(T));
        AddData(worksheet, data, 4);

        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            var dataRange = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];
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
            var displayAttr = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.GetCustomAttribute<DisplayAttribute>()?.Name;
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
                    cell.Style.Numberformat.Format = "yyyy-mm-dd";
                }
                else if (value is decimal dec)
                {
                    cell.Value = dec;
                    cell.Style.Numberformat.Format = "#,##0.00";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    cell.Value = value;
                }
                col++;
            }
            row++;
        }
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate, string? transactionType = null)
    {
        var parts = new List<string>();
        if (startDate.HasValue && endDate.HasValue) parts.Add($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        else if (startDate.HasValue) parts.Add($"From: {startDate:yyyy-MM-dd}");
        else if (endDate.HasValue) parts.Add($"Until: {endDate:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(transactionType)) parts.Add($"Type: {transactionType}");
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

    private string GetSubtitle(int? employeeId, string? department)
    {
        var parts = new List<string>();
        if (employeeId.HasValue) parts.Add($"Employee ID: {employeeId}");
        if (!string.IsNullOrEmpty(department)) parts.Add($"Department: {department}");
        return parts.Any() ? string.Join(" | ", parts) : "All Employees";
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate, bool? isUpcoming)
    {
        var parts = new List<string>();
        if (startDate.HasValue && endDate.HasValue) parts.Add($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        if (isUpcoming == true) parts.Add("Upcoming Only");
        return parts.Any() ? string.Join(" | ", parts) : "All Maintenance Records";
    }

    private string GetSubtitle(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue) return $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
        if (startDate.HasValue) return $"From: {startDate:yyyy-MM-dd}";
        if (endDate.HasValue) return $"Until: {endDate:yyyy-MM-dd}";
        return "All Records";
    }
}