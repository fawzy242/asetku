using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Reports.Interfaces;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Reports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportService;

    public ReportsController(IReportsService reportService) => _reportService = reportService;

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats() => this.HandleResult(await _reportService.GetDashboardStatsAsync());

    [HttpGet("asset-transaction/data")]
    public async Task<IActionResult> GetAssetTransactionData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? transactionType = null)
        => this.HandleResult(await _reportService.GetAssetTransactionReportsAsync(startDate, endDate, transactionType));

    [HttpGet("asset-transaction/excel")]
    public async Task<IActionResult> ExportAssetTransactionExcel([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? transactionType = null)
    {
        var result = await _reportService.GenerateAssetTransactionExcelAsync(startDate, endDate, transactionType);
        return result.IsSuccess ? File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Asset_Transaction_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx") : this.HandleResult(result);
    }

    [HttpGet("asset-inventory/data")]
    public async Task<IActionResult> GetAssetInventoryData([FromQuery] string? status = null, [FromQuery] int? categoryId = null, [FromQuery] int? supplierId = null)
        => this.HandleResult(await _reportService.GetAssetInventoryReportsAsync(status, categoryId, supplierId));

    [HttpGet("asset-inventory/excel")]
    public async Task<IActionResult> ExportAssetInventoryExcel([FromQuery] string? status = null, [FromQuery] int? categoryId = null, [FromQuery] int? supplierId = null)
    {
        var result = await _reportService.GenerateAssetInventoryExcelAsync(status, categoryId, supplierId);
        return result.IsSuccess ? File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Asset_Inventory_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx") : this.HandleResult(result);
    }

    [HttpGet("employee-asset/data")]
    public async Task<IActionResult> GetEmployeeAssetData([FromQuery] int? employeeId = null, [FromQuery] string? department = null)
        => this.HandleResult(await _reportService.GetEmployeeAssetReportsAsync(employeeId, department));

    [HttpGet("employee-asset/excel")]
    public async Task<IActionResult> ExportEmployeeAssetExcel([FromQuery] int? employeeId = null, [FromQuery] string? department = null)
    {
        var result = await _reportService.GenerateEmployeeAssetExcelAsync(employeeId, department);
        return result.IsSuccess ? File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employee_Asset_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx") : this.HandleResult(result);
    }

    [HttpGet("maintenance/data")]
    public async Task<IActionResult> GetMaintenanceData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] bool? isUpcoming = null)
        => this.HandleResult(await _reportService.GetMaintenanceReportsAsync(startDate, endDate, isUpcoming));

    [HttpGet("maintenance/excel")]
    public async Task<IActionResult> ExportMaintenanceExcel([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] bool? isUpcoming = null)
    {
        var result = await _reportService.GenerateMaintenanceExcelAsync(startDate, endDate, isUpcoming);
        var fileName = isUpcoming == true ? $"Upcoming_Maintenance_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" : $"Maintenance_History_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return result.IsSuccess ? File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName) : this.HandleResult(result);
    }

    [HttpGet("financial/data")]
    public async Task<IActionResult> GetFinancialData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        => this.HandleResult(await _reportService.GetFinancialReportsAsync(startDate, endDate));

    [HttpGet("financial/excel")]
    public async Task<IActionResult> ExportFinancialExcel([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _reportService.GenerateFinancialExcelAsync(startDate, endDate);
        return result.IsSuccess ? File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Financial_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx") : this.HandleResult(result);
    }
}