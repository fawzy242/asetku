using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Office;
using Whitebird.Domain.Features.Office;
using Whitebird.Features.Common;

namespace Whitebird.Features.Office;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OfficeController : ControllerBase
{
    private readonly IOfficeService _officeService;

    public OfficeController(IOfficeService officeService)
    {
        _officeService = officeService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _officeService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _officeService.GetAllAsync());

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveOnly()
        => this.HandleResult(await _officeService.GetActiveOnlyAsync());

    [HttpGet("suboffices/{parentId:int}")]
    public async Task<IActionResult> GetSubOffices(int parentId)
        => this.HandleResult(await _officeService.GetSubOfficesAsync(parentId));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => this.HandleResult(await _officeService.GetGridDataAsync(page, pageSize, search));

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownList()
        => this.HandleResult(await _officeService.GetDropdownListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OfficeCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _officeService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OfficeUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _officeService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _officeService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _officeService.SoftDeleteAsync(id));
}