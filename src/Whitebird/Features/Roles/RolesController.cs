using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Roles;
using Whitebird.Domain.Features.Roles;
using Whitebird.Features.Common;

namespace Whitebird.Features.Roles;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
// P2-3: role CRUD is restricted to administrator-level users. The claim value
// emitted by SessionAuthenticationHandler is RoleName (e.g. "Administrator");
// RoleCode ("ADMIN") is accepted as well for forward compatibility. Add other
// role codes here if additional privileged roles are introduced.
[Authorize(Roles = "ADMIN,Administrator")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _rolesService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _rolesService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RolesCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _rolesService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RolesUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _rolesService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _rolesService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _rolesService.SoftDeleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] RolesGridRequest request)
        => this.HandleResult(await _rolesService.GetGridDataAsync(request));

    // ============================================================
    // DROPDOWN
    // ============================================================

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownList()
        => this.HandleResult(await _rolesService.GetDropdownListAsync());

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _rolesService.BulkActivateAsync(request));
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _rolesService.BulkDeactivateAsync(request));
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _rolesService.BulkDeleteAsync(request));
    }
}