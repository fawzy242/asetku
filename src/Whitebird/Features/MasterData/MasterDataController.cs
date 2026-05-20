using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Features.Common;

namespace Whitebird.Features.MasterData.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGrouped()
        => this.HandleResult(await _masterDataService.GetAllGroupedAsync());

    [HttpGet("{referenceName}")]
    public async Task<IActionResult> GetByReferenceName(string referenceName)
        => this.HandleResult(await _masterDataService.GetByReferenceNameAsync(referenceName));

    [HttpGet("transaction-types")]
    public async Task<IActionResult> GetTransactionTypes()
        => this.HandleResult(await _masterDataService.GetTransactionTypesAsync());

    [HttpGet("asset-conditions")]
    public async Task<IActionResult> GetAssetConditions()
        => this.HandleResult(await _masterDataService.GetAssetConditionsAsync());

    [HttpGet("employee-positions")]
    public async Task<IActionResult> GetEmployeePositions()
        => this.HandleResult(await _masterDataService.GetEmployeePositionsAsync());

    [HttpGet("employee-statuses")]
    public async Task<IActionResult> GetEmployeeStatuses()
        => this.HandleResult(await _masterDataService.GetEmployeeStatusesAsync());

    [HttpGet("office-types")]
    public async Task<IActionResult> GetOfficeTypes()
        => this.HandleResult(await _masterDataService.GetOfficeTypesAsync());

    [HttpGet("maintenance-types")]
    public async Task<IActionResult> GetMaintenanceTypes()
        => this.HandleResult(await _masterDataService.GetMaintenanceTypesAsync());

    [HttpGet("asset-condition-purchases")]
    public async Task<IActionResult> GetAssetConditionPurchases()
        => this.HandleResult(await _masterDataService.GetAssetConditionPurchasesAsync());
}