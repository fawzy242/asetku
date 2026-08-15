using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Schedule;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Features.Common;

namespace Whitebird.Features.Schedule;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ScheduleReturnController : ControllerBase
{
    private readonly IScheduleReturnService _service;

    public ScheduleReturnController(IScheduleReturnService service)
    {
        _service = service;
    }

    // ============================================================
    // CRUD
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _service.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _service.GetAllAsync());

    [HttpGet("asset/{assetId:int}")]
    public async Task<IActionResult> GetByAssetId(int assetId)
        => this.HandleResult(await _service.GetByAssetIdAsync(assetId));

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
        => this.HandleResult(await _service.GetByEmployeeIdAsync(employeeId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScheduleReturnCreateDto dto)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _service.CreateAsync(dto), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ScheduleReturnUpdateDto dto)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _service.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _service.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _service.SoftDeleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] ScheduleGridRequest request)
        => this.HandleResult(await _service.GetGridDataAsync(request));

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _service.BulkActivateAsync(request));
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _service.BulkDeactivateAsync(request));
    }

    // ============================================================
    // GENERATE
    // ============================================================

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] ScheduleReturnGenerateDto dto)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _service.GenerateAsync(dto));
    }
}