using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Reports;
using Whitebird.Domain.Features.Reports;
using Whitebird.Features.Common;

namespace Whitebird.Features.Reports;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportService;

    public ReportsController(IReportsService reportService)
    {
        _reportService = reportService;
    }

    // ============================================================
    // DASHBOARD
    // ============================================================

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
        => this.HandleResult(await _reportService.GetDashboardStatsAsync());

    // ============================================================
    // ASSET TRANSACTION REPORT
    // ============================================================

    [HttpGet("asset-transaction/data")]
    public async Task<IActionResult> GetAssetTransactionData(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] string? transactionType = null)
        => this.HandleResult(await _reportService.GetAssetTransactionReportsAsync(startDate, endDate, transactionType));

    [HttpGet("asset-transaction/excel")]
    public async Task<IActionResult> ExportAssetTransactionExcel(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] string? transactionType = null)
    {
        var result = await _reportService.GenerateAssetTransactionExcelAsync(startDate, endDate, transactionType);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);
        
        var fileName = $"Asset_Transaction_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // ASSET INVENTORY REPORT
    // ============================================================

    [HttpGet("asset-inventory/data")]
    public async Task<IActionResult> GetAssetInventoryData(
        [FromQuery] string? status = null, 
        [FromQuery] int? categoryId = null, 
        [FromQuery] int? supplierId = null)
        => this.HandleResult(await _reportService.GetAssetInventoryReportsAsync(status, categoryId, supplierId));

    [HttpGet("asset-inventory/excel")]
    public async Task<IActionResult> ExportAssetInventoryExcel(
        [FromQuery] string? status = null, 
        [FromQuery] int? categoryId = null, 
        [FromQuery] int? supplierId = null)
    {
        var result = await _reportService.GenerateAssetInventoryExcelAsync(status, categoryId, supplierId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);
        
        var fileName = $"Asset_Inventory_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // EMPLOYEE ASSET REPORT
    // ============================================================

    [HttpGet("employee-asset/data")]
    public async Task<IActionResult> GetEmployeeAssetData(
        [FromQuery] int? employeeId = null)
        => this.HandleResult(await _reportService.GetEmployeeAssetReportsAsync(employeeId));

    [HttpGet("employee-asset/excel")]
    public async Task<IActionResult> ExportEmployeeAssetExcel(
        [FromQuery] int? employeeId = null)
    {
        var result = await _reportService.GenerateEmployeeAssetExcelAsync(employeeId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);
        
        var fileName = $"Employee_Asset_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // MAINTENANCE REPORT
    // ============================================================

    [HttpGet("maintenance/data")]
    public async Task<IActionResult> GetMaintenanceData(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] bool? isUpcoming = null)
        => this.HandleResult(await _reportService.GetMaintenanceReportsAsync(startDate, endDate, isUpcoming));

    [HttpGet("maintenance/excel")]
    public async Task<IActionResult> ExportMaintenanceExcel(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] bool? isUpcoming = null)
    {
        var result = await _reportService.GenerateMaintenanceExcelAsync(startDate, endDate, isUpcoming);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);
        
        var reportType = isUpcoming == true ? "Upcoming_Maintenance" : "Maintenance_History";
        var fileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // FINANCIAL REPORT
    // ============================================================

    [HttpGet("financial/data")]
    public async Task<IActionResult> GetFinancialData(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
        => this.HandleResult(await _reportService.GetFinancialReportsAsync(startDate, endDate));

    [HttpGet("financial/excel")]
    public async Task<IActionResult> ExportFinancialExcel(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _reportService.GenerateFinancialExcelAsync(startDate, endDate);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);
        
        var fileName = $"Financial_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}