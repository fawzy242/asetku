using System.Text;
using System.Text.RegularExpressions;
using Dapper;

namespace Whitebird.Infra.Features.Common;

/// <summary>
/// Helper class for building grid queries with pagination, filtering, and sorting
/// </summary>
public static class GridQueryBuilder
{
    /// <summary>
    /// Upper bound for a client-supplied page size. Prevents DOS via
    /// <c>pageSize=int.MaxValue</c> on grid endpoints while still allowing
    /// legitimate large pages (e.g. 100, 200, 500 rows).
    ///
    /// <see cref="int.MaxValue"/> is treated as an explicit "unbounded"
    /// sentinel for internal callers (e.g. <c>AssetTransactionService.GetAllAsync</c>)
    /// and bypasses the clamp. No external client should ever pass it.
    /// </summary>
    public const int MaxPageSize = 500;

    /// <summary>
    /// Fallback page size for callers that pass <c>pageSize &lt;= 0</c>.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Clamps a client-supplied page size to <see cref="MaxPageSize"/>. Preserves
    /// the <see cref="int.MaxValue"/> "unbounded" sentinel used internally.
    /// </summary>
    public static int ClampPageSize(int pageSize)
    {
        if (pageSize <= 0) return DefaultPageSize;
        if (pageSize == int.MaxValue) return int.MaxValue;
        return Math.Min(pageSize, MaxPageSize);
    }

    /// <summary>
    /// Pattern for a safe sort-column reference: "ColumnName" or "Table.ColumnName".
    /// Blocks SQL injection through SortBy (semicolons, comments, quotes, function
    /// calls, commas, subqueries) while still allowing any legitimate column reference
    /// the caller may pass. Case-insensitive.
    /// </summary>
    private static readonly Regex _safeSortColumnPattern = new(
        @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string BuildWhereClause(List<string> conditions, DynamicParameters parameters)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return string.Empty;
        }

        return $"WHERE {string.Join(" AND ", conditions)}";
    }

    /// <summary>
    /// Build ORDER BY clause. Sanitises user-supplied <paramref name="sortBy"/>
    /// against a strict column-reference pattern; anything unsafe falls back to
    /// <paramref name="defaultSortBy"/>.
    /// </summary>
    public static string BuildOrderByClause(string? sortBy, bool sortDescending, string defaultSortBy, bool defaultDescending = true)
    {
        var actualSortBy = string.IsNullOrEmpty(sortBy) ? defaultSortBy : sortBy;
        var actualDescending = string.IsNullOrEmpty(sortBy) ? defaultDescending : sortDescending;

        if (!IsSafeSortColumn(actualSortBy))
        {
            actualSortBy = defaultSortBy;
            actualDescending = defaultDescending;
        }

        return $"ORDER BY {actualSortBy} {(actualDescending ? "DESC" : "ASC")}";
    }

    /// <summary>
    /// Returns true when <paramref name="column"/> is a plain column reference
    /// (<c>ColumnName</c> or <c>Table.ColumnName</c>).
    /// </summary>
    public static bool IsSafeSortColumn(string? column)
    {
        if (string.IsNullOrWhiteSpace(column)) return false;
        return _safeSortColumnPattern.IsMatch(column);
    }

    /// <summary>
    /// Build pagination clause (OFFSET FETCH). Page size is clamped via
    /// <see cref="ClampPageSize"/>; the <see cref="int.MaxValue"/> sentinel is
    /// preserved so internal "get all" callers keep working.
    /// </summary>
    public static string BuildPaginationClause(int page, int pageSize, DynamicParameters parameters)
    {
        var effectivePageSize = ClampPageSize(pageSize);
        var effectivePage = page <= 0 ? 1 : page;

        if (effectivePageSize == int.MaxValue)
        {
            // Sentinel: unbounded fetch (used by AssetTransactionService.GetAllAsync).
            parameters.Add("@Offset", 0);
            parameters.Add("@PageSize", int.MaxValue);
        }
        else
        {
            parameters.Add("@Offset", (effectivePage - 1) * effectivePageSize);
            parameters.Add("@PageSize", effectivePageSize);
        }

        return "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
    }

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