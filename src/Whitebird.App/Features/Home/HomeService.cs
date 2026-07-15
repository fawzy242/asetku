using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Home;
using Whitebird.Infra.Features.Home;

namespace Whitebird.App.Features.Home;

/// <summary>
/// Service implementation for Home business logic
/// </summary>
public class HomeService : BaseService, IHomeService
{
    private readonly IHomeReps _homeReps;

    public HomeService(
        IHomeReps homeReps,
        ILogger<HomeService> logger) : base(logger)
    {
        _homeReps = homeReps;
    }

    public async Task<ServiceResult<HomeSearchResultDto>> SearchAsync(string keyword)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            // Validate keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return ServiceResult<HomeSearchResultDto>.BadRequest("Keyword is required");
            }

            if (keyword.Length < 2)
            {
                return ServiceResult<HomeSearchResultDto>.BadRequest("Keyword must be at least 2 characters");
            }

            // Search
            var results = await _homeReps.SearchAsync(keyword);
            var resultList = results.ToList();

            var response = new HomeSearchResultDto
            {
                Keyword = keyword,
                TotalResults = resultList.Count,
                Results = resultList
            };

            return ServiceResult<HomeSearchResultDto>.Success(response);
        }, "search");
    }
}