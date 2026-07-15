using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Reports;
using Whitebird.Features.Common;

namespace Whitebird.Features.Reports;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    // ============================================================
    // LIST ENDPOINTS (for dropdown parameters)
    // ============================================================

    [HttpGet("employee-list")]
    public async Task<IActionResult> GetEmployeeList()
        => this.HandleResult(await _reportsService.GetEmployeeListAsync());

    [HttpGet("category-list")]
    public async Task<IActionResult> GetCategoryList()
        => this.HandleResult(await _reportsService.GetCategoryListAsync());

    [HttpGet("supplier-list")]
    public async Task<IActionResult> GetSupplierList()
        => this.HandleResult(await _reportsService.GetSupplierListAsync());

    [HttpGet("transaction-type-list")]
    public async Task<IActionResult> GetTransactionTypeList()
        => this.HandleResult(await _reportsService.GetTransactionTypeListAsync());

    [HttpGet("maintenance-type-list")]
    public async Task<IActionResult> GetMaintenanceTypeList()
        => this.HandleResult(await _reportsService.GetMaintenanceTypeListAsync());

    [HttpGet("condition-list")]
    public async Task<IActionResult> GetConditionList()
        => this.HandleResult(await _reportsService.GetConditionListAsync());

    [HttpGet("status-list")]
    public async Task<IActionResult> GetStatusList()
        => this.HandleResult(await _reportsService.GetStatusListAsync());

    // ============================================================
    // EXPORT ENDPOINTS
    // ============================================================

    [HttpGet("asset-transaction/export")]
    public async Task<IActionResult> ExportAssetTransaction(
        [FromQuery] string format,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? transactionType = null)
    {
        var result = await _reportsService.ExportAssetTransactionAsync(format, startDate, endDate, transactionType);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var fileName = $"Asset_Transaction_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
        return File(result.Data, GetContentType(format), fileName);
    }

    [HttpGet("asset-inventory/export")]
    public async Task<IActionResult> ExportAssetInventory(
        [FromQuery] string format,
        [FromQuery] string? status = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? supplierId = null)
    {
        var result = await _reportsService.ExportAssetInventoryAsync(format, status, categoryId, supplierId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var fileName = $"Asset_Inventory_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
        return File(result.Data, GetContentType(format), fileName);
    }

    [HttpGet("employee-asset/export")]
    public async Task<IActionResult> ExportEmployeeAsset(
        [FromQuery] string format,
        [FromQuery] int? employeeId = null)
    {
        var result = await _reportsService.ExportEmployeeAssetAsync(format, employeeId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var fileName = $"Employee_Asset_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
        return File(result.Data, GetContentType(format), fileName);
    }

    [HttpGet("maintenance/export")]
    public async Task<IActionResult> ExportMaintenance(
        [FromQuery] string format,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? isUpcoming = null)
    {
        var result = await _reportsService.ExportMaintenanceAsync(format, startDate, endDate, isUpcoming);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var reportType = isUpcoming == true ? "Upcoming_Maintenance" : "Maintenance_History";
        var fileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
        return File(result.Data, GetContentType(format), fileName);
    }

    [HttpGet("financial/export")]
    public async Task<IActionResult> ExportFinancial(
        [FromQuery] string format,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _reportsService.ExportFinancialAsync(format, startDate, endDate);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var fileName = $"Financial_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
        return File(result.Data, GetContentType(format), fileName);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private string GetFileExtension(string format)
    {
        return format.ToLower() == "pdf" ? "pdf" : "xlsx";
    }

    private string GetContentType(string format)
    {
        return format.ToLower() == "pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }
}