using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Asset.Interfaces;
using Whitebird.Domain.Features.Asset.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Asset.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService) => _assetService = assetService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _assetService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _assetService.GetAllAsync());

    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategory(int categoryId) => this.HandleResult(await _assetService.GetByCategoryAsync(categoryId));

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status) => this.HandleResult(await _assetService.GetByStatusAsync(status));

    [HttpGet("holder/{employeeId:int}")]
    public async Task<IActionResult> GetByHolder(int employeeId) => this.HandleResult(await _assetService.GetByHolderAsync(employeeId));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] bool sortDescending = false)
        => this.HandleResult(await _assetService.GetGridDataAsync(page, pageSize, search, sortBy, sortDescending));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword) => this.HandleResult(await _assetService.SearchAsync(keyword));

    [HttpGet("expired-warranty")]
    public async Task<IActionResult> GetExpiredWarranty() => this.HandleResult(await _assetService.GetExpiredWarrantyAsync());

    [HttpGet("upcoming-maintenance")]
    public async Task<IActionResult> GetUpcomingMaintenance([FromQuery] int daysAhead = 30) => this.HandleResult(await _assetService.GetUpcomingMaintenanceAsync(daysAhead));

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats() => this.HandleResult(await _assetService.GetDashboardStatsAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => this.HandleResult(await _assetService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id) => this.HandleResult(await _assetService.SoftDeleteAsync(id));
}