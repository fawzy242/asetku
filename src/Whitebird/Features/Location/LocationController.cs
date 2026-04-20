using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Location.Interfaces;
using Whitebird.Domain.Features.Location.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Location.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService) => _locationService = locationService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _locationService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _locationService.GetAllAsync());

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveOnly() => this.HandleResult(await _locationService.GetActiveOnlyAsync());

    [HttpGet("sublocations/{parentId:int}")]
    public async Task<IActionResult> GetSubLocations(int parentId) => this.HandleResult(await _locationService.GetSubLocationsAsync(parentId));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => this.HandleResult(await _locationService.GetGridDataAsync(page, pageSize, search));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LocationCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _locationService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LocationUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _locationService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => this.HandleResult(await _locationService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id) => this.HandleResult(await _locationService.SoftDeleteAsync(id));
}