using System.Text;
using Dapper;

namespace Whitebird.Infra.Features.Common;

/// <summary>
/// Helper class for building grid queries with pagination, filtering, and sorting
/// </summary>
public static class GridQueryBuilder
{
    /// <summary>
    /// Build WHERE clause from filter conditions
    /// </summary>
    /// <param name="conditions">List of SQL conditions</param>
    /// <param name="parameters">DynamicParameters to add parameter values</param>
    /// <returns>WHERE clause string</returns>
    public static string BuildWhereClause(List<string> conditions, DynamicParameters parameters)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return string.Empty;
        }

        return $"WHERE {string.Join(" AND ", conditions)}";
    }

    /// <summary>
    /// Build ORDER BY clause
    /// </summary>
    /// <param name="sortBy">Column name to sort by</param>
    /// <param name="sortDescending">True for descending, false for ascending</param>
    /// <param name="defaultSortBy">Default column name if sortBy is null or empty</param>
    /// <param name="defaultDescending">Default sort direction if sortBy is null or empty</param>
    /// <returns>ORDER BY clause string</returns>
    public static string BuildOrderByClause(string? sortBy, bool sortDescending, string defaultSortBy, bool defaultDescending = true)
    {
        var actualSortBy = string.IsNullOrEmpty(sortBy) ? defaultSortBy : sortBy;
        var actualDescending = string.IsNullOrEmpty(sortBy) ? defaultDescending : sortDescending;

        return $"ORDER BY {actualSortBy} {(actualDescending ? "DESC" : "ASC")}";
    }

    /// <summary>
    /// Build pagination clause (OFFSET FETCH)
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="parameters">DynamicParameters to add offset and pageSize</param>
    /// <returns>OFFSET FETCH clause string</returns>
    public static string BuildPaginationClause(int page, int pageSize, DynamicParameters parameters)
    {
        var offset = (page - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        return "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
    }

    /// <summary>
    /// Build complete paginated query with WHERE, ORDER BY, and OFFSET FETCH
    /// </summary>
    public static (string DataQuery, string CountQuery, DynamicParameters Parameters) BuildPaginatedQuery(
        string selectClause,
        string fromClause,
        List<string> conditions,
        string? sortBy,
        bool sortDescending,
        string defaultSortBy,
        bool defaultDescending,
        int page,
        int pageSize,
        DynamicParameters? existingParameters = null)
    {
        var parameters = existingParameters ?? new DynamicParameters();

        var whereClause = BuildWhereClause(conditions, parameters);
        var orderByClause = BuildOrderByClause(sortBy, sortDescending, defaultSortBy, defaultDescending);
        var paginationClause = BuildPaginationClause(page, pageSize, parameters);

        var dataQuery = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderByClause}
            {paginationClause}";

        var countQuery = $@"
            SELECT COUNT(*)
            {fromClause}
            {whereClause}";

        return (dataQuery, countQuery, parameters);
    }

    /// <summary>
    /// Add search condition with multiple columns
    /// </summary>
    public static void AddSearchCondition(List<string> conditions, DynamicParameters parameters, string? search, string[] columns)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return;
        }

        var searchPattern = $"%{search}%";
        var searchConditions = columns.Select(c => $"{c} LIKE @Search");
        conditions.Add($"({string.Join(" OR ", searchConditions)})");
        parameters.Add("@Search", searchPattern);
    }

    /// <summary>
    /// Add equals filter condition
    /// </summary>
    public static void AddEqualsFilter<T>(List<string> conditions, DynamicParameters parameters, string column, T? value)
    {
        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            return;
        }

        var paramName = $"@{column.Replace(".", "_")}";
        conditions.Add($"{column} = {paramName}");
        parameters.Add(paramName, value);
    }

    /// <summary>
    /// Add date range filter condition
    /// </summary>
    public static void AddDateRangeFilter(List<string> conditions, DynamicParameters parameters, string column, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            var paramName = $"@{column.Replace(".", "_")}_From";
            conditions.Add($"{column} >= {paramName}");
            parameters.Add(paramName, from.Value);
        }

        if (to.HasValue)
        {
            var paramName = $"@{column.Replace(".", "_")}_To";
            conditions.Add($"{column} <= {paramName}");
            parameters.Add(paramName, to.Value);
        }
    }

    /// <summary>
    /// Add IN filter condition
    /// </summary>
    public static void AddInFilter<T>(List<string> conditions, DynamicParameters parameters, string column, IEnumerable<T>? values)
    {
        if (values == null || !values.Any())
        {
            return;
        }

        var valueList = values.ToList();
        var paramName = $"@{column.Replace(".", "_")}_List";
        conditions.Add($"{column} IN {paramName}");
        parameters.Add(paramName, valueList);
    }
}