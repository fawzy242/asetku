using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Home;

namespace Whitebird.App.Features.Home;

/// <summary>
/// Service interface for Home business logic
/// </summary>
public interface IHomeService
{
    /// <summary>
    /// Search assets and employees by keyword
    /// </summary>
    /// <param name="keyword">Search keyword (min 2 characters)</param>
    /// <returns>Search results</returns>
    Task<ServiceResult<HomeSearchResultDto>> SearchAsync(string keyword);
}