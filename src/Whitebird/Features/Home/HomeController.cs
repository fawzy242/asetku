using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Home;
using Whitebird.Features.Common;

namespace Whitebird.Features.Home;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    // ============================================================
    // SEARCH
    // ============================================================

    /// <summary>
    /// Global search for Assets and Employees
    /// </summary>
    /// <param name="keyword">Search keyword (min 2 characters)</param>
    /// <returns>Search results</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ServiceResult<object>.BadRequest("Keyword is required"));
        }

        if (keyword.Length < 2)
        {
            return BadRequest(ServiceResult<object>.BadRequest("Keyword must be at least 2 characters"));
        }

        return this.HandleResult(await _homeService.SearchAsync(keyword));
    }
}