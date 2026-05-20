using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.Domain.Features.Asset;
using Whitebird.Features.Common;

namespace Whitebird.Features.Asset.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly AssetImportService _assetImportService;

    public AssetController(IAssetService assetService, AssetImportService assetImportService)
    {
        _assetService = assetService;
        _assetImportService = assetImportService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _assetService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _assetService.GetAllAsync());

    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
        => this.HandleResult(await _assetService.GetByCategoryAsync(categoryId));

    [HttpGet("office/{officeId:int}")]
    public async Task<IActionResult> GetByOffice(int officeId)
        => this.HandleResult(await _assetService.GetByOfficeAsync(officeId));

    [HttpGet("tracking/{assetId:int}")]
    public async Task<IActionResult> GetAssetTracking(int assetId)
        => this.HandleResult(await _assetService.GetAssetTrackingAsync(assetId));

    [HttpGet("status/{assetId:int}")]
    public async Task<IActionResult> GetCurrentStatus(int assetId)
        => this.HandleResult(await _assetService.GetCurrentStatusAsync(assetId));

    [HttpGet("history/{assetId:int}")]
    public async Task<IActionResult> GetTransactionHistory(int assetId)
        => this.HandleResult(await _assetService.GetAssetTransactionHistoryAsync(assetId));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] bool sortDescending = false)
        => this.HandleResult(await _assetService.GetGridDataAsync(page, pageSize, search, sortBy, sortDescending));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
        => this.HandleResult(await _assetService.SearchAsync(keyword));

    [HttpGet("expired-warranty")]
    public async Task<IActionResult> GetExpiredWarranty()
        => this.HandleResult(await _assetService.GetExpiredWarrantyAsync());

    [HttpGet("upcoming-maintenance")]
    public async Task<IActionResult> GetUpcomingMaintenance([FromQuery] int daysAhead = 30)
        => this.HandleResult(await _assetService.GetUpcomingMaintenanceAsync(daysAhead));

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
        => this.HandleResult(await _assetService.GetDashboardStatsAsync());

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
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _assetService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _assetService.SoftDeleteAsync(id));

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.BulkActivateAsync(request));
    }

[HttpPost("import")]
public async Task<IActionResult> Import(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("No file provided");
    
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    
    using var stream = file.OpenReadStream();
    ServiceResult<ImportResult> result;
    
    if (extension == ".txt" || extension == ".csv")
    {
        result = await _assetImportService.ImportFromTxtAsync(stream);
    }
    else if (extension == ".xlsx" || extension == ".xls")
    {
        result = await _assetImportService.ImportFromExcelAsync(stream);
    }
    else
    {
        return BadRequest($"Unsupported file format: {extension}. Supported: .xlsx, .xls, .csv, .txt");
    }
    
    return this.HandleResult(result);
}

    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _assetImportService.GenerateTemplateAsync();
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Asset_Import_Template.xlsx");
    }
}