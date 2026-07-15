namespace Whitebird.Domain.Features.Common;

/// <summary>
/// Base class for grid request with common filter parameters
/// </summary>
public abstract class GridRequestBase
{
    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search keyword for free text search
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by IsActive status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Sort column name
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true = descending, false = ascending)
    /// </summary>
    public bool SortDescending { get; set; } = true;
}