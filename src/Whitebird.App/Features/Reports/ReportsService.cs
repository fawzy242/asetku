using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.Reports.Draw;
using Whitebird.App.Features.Supplier;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Reports;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Reports;

namespace Whitebird.App.Features.Reports;

/// <summary>
/// Service implementation for Reports business logic
/// </summary>
public class ReportsService : BaseService, IReportsService
{
    private readonly IReportsReps _reportsReps;
    private readonly IEmployeeService _employeeService;
    private readonly ICategoryService _categoryService;
    private readonly ISupplierService _supplierService;
    private readonly IMasterDataService _masterDataService;
    private readonly DrawReportAssetTransaction _drawAssetTransaction;
    private readonly DrawReportAssetInventory _drawAssetInventory;
    private readonly DrawReportEmployeeAsset _drawEmployeeAsset;
    private readonly DrawReportMaintenance _drawMaintenance;
    private readonly DrawReportFinancial _drawFinancial;

    public ReportsService(
        IReportsReps reportsReps,
        IEmployeeService employeeService,
        ICategoryService categoryService,
        ISupplierService supplierService,
        IMasterDataService masterDataService,
        DrawReportAssetTransaction drawAssetTransaction,
        DrawReportAssetInventory drawAssetInventory,
        DrawReportEmployeeAsset drawEmployeeAsset,
        DrawReportMaintenance drawMaintenance,
        DrawReportFinancial drawFinancial,
        ILogger<ReportsService> logger) : base(logger)
    {
        _reportsReps = reportsReps;
        _employeeService = employeeService;
        _categoryService = categoryService;
        _supplierService = supplierService;
        _masterDataService = masterDataService;
        _drawAssetTransaction = drawAssetTransaction;
        _drawAssetInventory = drawAssetInventory;
        _drawEmployeeAsset = drawEmployeeAsset;
        _drawMaintenance = drawMaintenance;
        _drawFinancial = drawFinancial;
    }

    // ============================================================
    // LIST ENDPOINTS
    // ============================================================

    public async Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetEmployeeListAsync()
    {
        return await _employeeService.GetDropdownListAsync();
    }

    public async Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetCategoryListAsync()
    {
        return await _categoryService.GetDropdownListAsync();
    }

    public async Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetSupplierListAsync()
    {
        return await _supplierService.GetDropdownListAsync();
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeListAsync()
    {
        return await _masterDataService.GetTransactionTypesAsync();
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypeListAsync()
    {
        return await _masterDataService.GetMaintenanceTypesAsync();
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetConditionListAsync()
    {
        return await _masterDataService.GetAssetConditionsAsync();
    }

    public async Task<ServiceResult<IEnumerable<StatusListDto>>> GetStatusListAsync()
    {
        var statuses = new List<StatusListDto>
        {
            new() { Value = "Available", Label = "Available" },
            new() { Value = "Assigned", Label = "Assigned" },
            new() { Value = "On Loan", Label = "On Loan" },
            new() { Value = "In Maintenance", Label = "In Maintenance" },
            new() { Value = "Damaged", Label = "Damaged" },
            new() { Value = "Active", Label = "Active" },
            new() { Value = "Inactive", Label = "Inactive" }
        };
        return ServiceResult<IEnumerable<StatusListDto>>.Success(statuses);
    }

    // ============================================================
    // EXPORT ENDPOINTS
    // ============================================================

    public async Task<ServiceResult<byte[]>> ExportAssetTransactionAsync(
        string format, DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (!IsValidFormat(format))
                return ServiceResult<byte[]>.BadRequest($"Invalid format: {format}. Valid: excel, pdf");

            var data = await _reportsReps.GetAssetTransactionDataAsync(startDate, endDate, transactionType);
            var bytes = await _drawAssetTransaction.GenerateAsync(data, format);

            return ServiceResult<byte[]>.Success(bytes, $"Asset Transaction Report generated successfully");
        }, "export asset transaction report");
    }

    public async Task<ServiceResult<byte[]>> ExportAssetInventoryAsync(
        string format, string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (!IsValidFormat(format))
                return ServiceResult<byte[]>.BadRequest($"Invalid format: {format}. Valid: excel, pdf");

            var data = await _reportsReps.GetAssetInventoryDataAsync(status, categoryId, supplierId);
            var bytes = await _drawAssetInventory.GenerateAsync(data, format);

            return ServiceResult<byte[]>.Success(bytes, $"Asset Inventory Report generated successfully");
        }, "export asset inventory report");
    }

    public async Task<ServiceResult<byte[]>> ExportEmployeeAssetAsync(
        string format, int? employeeId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (!IsValidFormat(format))
                return ServiceResult<byte[]>.BadRequest($"Invalid format: {format}. Valid: excel, pdf");

            var data = await _reportsReps.GetEmployeeAssetDataAsync(employeeId);
            var bytes = await _drawEmployeeAsset.GenerateAsync(data, format);

            return ServiceResult<byte[]>.Success(bytes, $"Employee Asset Report generated successfully");
        }, "export employee asset report");
    }

    public async Task<ServiceResult<byte[]>> ExportMaintenanceAsync(
        string format, DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (!IsValidFormat(format))
                return ServiceResult<byte[]>.BadRequest($"Invalid format: {format}. Valid: excel, pdf");

            var data = await _reportsReps.GetMaintenanceDataAsync(startDate, endDate, isUpcoming);
            var bytes = await _drawMaintenance.GenerateAsync(data, format);

            return ServiceResult<byte[]>.Success(bytes, $"Maintenance Report generated successfully");
        }, "export maintenance report");
    }

    public async Task<ServiceResult<byte[]>> ExportFinancialAsync(
        string format, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (!IsValidFormat(format))
                return ServiceResult<byte[]>.BadRequest($"Invalid format: {format}. Valid: excel, pdf");

            var data = await _reportsReps.GetFinancialDataAsync(startDate, endDate);
            var bytes = await _drawFinancial.GenerateAsync(data, format);

            return ServiceResult<byte[]>.Success(bytes, $"Financial Report generated successfully");
        }, "export financial report");
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private bool IsValidFormat(string format)
    {
        return format?.ToLower() == "excel" || format?.ToLower() == "pdf";
    }
}

/// <summary>
/// DTO for status list in reports
/// </summary>
public class StatusListDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}