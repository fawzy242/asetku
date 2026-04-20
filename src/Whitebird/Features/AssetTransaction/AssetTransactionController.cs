using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.AssetTransaction.Interfaces;
using Whitebird.Domain.Features.AssetTransaction.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.AssetTransaction.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetTransactionController : ControllerBase
{
    private readonly IAssetTransactionService _transactionService;

    public AssetTransactionController(IAssetTransactionService transactionService) => _transactionService = transactionService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _transactionService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _transactionService.GetAllAsync());

    [HttpGet("asset/{assetId:int}")]
    public async Task<IActionResult> GetByAssetId(int assetId) => this.HandleResult(await _transactionService.GetByAssetIdAsync(assetId));

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId) => this.HandleResult(await _transactionService.GetByEmployeeIdAsync(employeeId));

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status) => this.HandleResult(await _transactionService.GetByStatusAsync(status));

    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals() => this.HandleResult(await _transactionService.GetPendingApprovalsAsync());

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] int? assetId = null)
        => this.HandleResult(await _transactionService.GetGridDataAsync(page, pageSize, search, status, assetId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetTransactionCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetTransactionUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UpdateAsync(id, model));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] AssetTransactionApproveViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.ApproveAsync(id, model));
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnAsset([FromBody] AssetReturnViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.ReturnAssetAsync(model));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id) => this.HandleResult(await _transactionService.CancelAsync(id));
}