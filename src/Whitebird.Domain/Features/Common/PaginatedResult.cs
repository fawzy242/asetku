namespace Whitebird.Domain.Features.Common;

/// <summary>
/// Paginated result wrapper for grid responses
/// </summary>
/// <typeparam name="T">Type of data items</typeparam>
public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public Dictionary<string, object>? Filters { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}