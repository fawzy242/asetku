using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.MasterData;

namespace Whitebird.Controllers.MasterData;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BatchMasterDataController : ControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public BatchMasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    /// <summary>
    /// Get multiple master data types in a single request
    /// </summary>
    /// <param name="names">Comma-separated list of reference names (e.g., "TransactionType,AssetCondition,Position")</param>
    /// <returns>Dictionary of reference name to list of master data items</returns>
    [HttpGet]
    public async Task<IActionResult> GetBatch([FromQuery] string names)
    {
        if (string.IsNullOrWhiteSpace(names))
        {
            return BadRequest(ServiceResult<object>.Failure("At least one reference name is required"));
        }

        var nameList = names.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .Distinct()
            .ToList();

        var result = new Dictionary<string, IEnumerable<MasterDataDto>>();

        foreach (var name in nameList)
        {
            var dataResult = await _masterDataService.GetByReferenceNameAsync(name);
            if (dataResult.IsSuccess && dataResult.Data != null)
            {
                result[name] = dataResult.Data;
            }
            else
            {
                result[name] = new List<MasterDataDto>();
            }
        }

        return Ok(ServiceResult<Dictionary<string, IEnumerable<MasterDataDto>>>.Success(result));
    }
}