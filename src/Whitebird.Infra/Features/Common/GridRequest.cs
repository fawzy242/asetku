namespace Whitebird.Infra.Features.Common;

public class GridRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string SortDirection { get; set; } = "ASC";
    public Dictionary<string, string> Filters { get; set; } = new();
    public string? SearchTerm { get; set; }

    public bool IsValidSortColumn(string[] validColumns)
    {
        return !string.IsNullOrEmpty(SortColumn) && validColumns.Contains(SortColumn, StringComparer.OrdinalIgnoreCase);
    }

    public string GetSortColumn(string defaultColumn) => string.IsNullOrEmpty(SortColumn) ? defaultColumn : SortColumn;

    public bool IsSortDescending() => SortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase);
}

public class GridResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static GridResponse<T> FromPaginatedResult(PaginatedResult<T> result)
    {
        return new GridResponse<T>
        {
            Data = result.Data,
            TotalCount = result.TotalCount,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}