using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Supplier;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Features.Common;

namespace Whitebird.Features.Supplier;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _supplierService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _supplierService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _supplierService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _supplierService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _supplierService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _supplierService.SoftDeleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] SupplierGridRequest request)
        => this.HandleResult(await _supplierService.GetGridDataAsync(request));

    // ============================================================
    // DROPDOWN
    // ============================================================

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownList()
        => this.HandleResult(await _supplierService.GetDropdownListAsync());

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _supplierService.BulkActivateAsync(request));
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _supplierService.BulkDeactivateAsync(request));
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _supplierService.BulkDeleteAsync(request));
    }
}