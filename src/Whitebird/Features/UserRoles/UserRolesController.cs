using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.UserRoles;
using Whitebird.Domain.Features.UserRoles;
using Whitebird.Features.Common;

namespace Whitebird.Features.UserRoles;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UserRolesController : ControllerBase
{
    private readonly IUserRolesService _userRolesService;

    public UserRolesController(IUserRolesService userRolesService)
    {
        _userRolesService = userRolesService;
    }

    // ============================================================
    // READS
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] UserRolesGridRequest request)
        => this.HandleResult(await _userRolesService.GetGridDataAsync(request));

    [HttpGet("by-user/{userId:int}")]
    public async Task<IActionResult> GetRolesByUserId(int userId)
        => this.HandleResult(await _userRolesService.GetRolesByUserIdAsync(userId));

    [HttpGet("by-role/{roleId:int}")]
    public async Task<IActionResult> GetUsersByRoleId(int roleId)
        => this.HandleResult(await _userRolesService.GetUsersByRoleIdAsync(roleId));

    // ============================================================
    // ASSIGN / UNASSIGN
    // ============================================================

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] UserRolesAssignRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _userRolesService.AssignAsync(request));
    }

    [HttpDelete("user/{userId:int}/role/{roleId:int}")]
    public async Task<IActionResult> Unassign(int userId, int roleId)
        => this.HandleResult(await _userRolesService.UnassignAsync(userId, roleId));

    // ============================================================
    // REPLACE ALL ROLES FOR A USER
    // ============================================================

    [HttpPut("user/{userId:int}")]
    public async Task<IActionResult> ReplaceForUser(int userId, [FromBody] UserRolesReplaceRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _userRolesService.ReplaceForUserAsync(userId, request));
    }
}