using Whitebird.Domain.Features.Home;

namespace Whitebird.Infra.Features.Home;

/// <summary>
/// Repository interface for Home operations
/// </summary>
public interface IHomeReps
{
    /// <summary>
    /// Search assets and employees by keyword
    /// </summary>
    /// <param name="keyword">Search keyword (min 2 characters)</param>
    /// <returns>List of search results</returns>
    Task<IEnumerable<HomeSearchItemDto>> SearchAsync(string keyword);
}