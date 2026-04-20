using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Supplier.Interfaces;
using Whitebird.Domain.Features.Supplier.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Supplier.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService) => _supplierService = supplierService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _supplierService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _supplierService.GetAllAsync());

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveOnly() => this.HandleResult(await _supplierService.GetActiveOnlyAsync());

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => this.HandleResult(await _supplierService.GetGridDataAsync(page, pageSize, search));

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
    public async Task<IActionResult> Delete(int id) => this.HandleResult(await _supplierService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id) => this.HandleResult(await _supplierService.SoftDeleteAsync(id));
}