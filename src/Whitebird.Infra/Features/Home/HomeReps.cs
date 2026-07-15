using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Home;

namespace Whitebird.Infra.Features.Home;

/// <summary>
/// Repository implementation for Home operations using Dapper
/// </summary>
public class HomeReps : IHomeReps
{
    private readonly DapperContext _context;

    public HomeReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HomeSearchItemDto>> SearchAsync(string keyword)
    {
        var results = new List<HomeSearchItemDto>();
        var searchPattern = $"%{keyword}%";

        // ============================================================
        // SEARCH ASSETS
        // ============================================================

        const string assetSql = @"
            SELECT TOP 10
                'Asset' as Type,
                a.AssetId as Id,
                a.AssetCode as Code,
                a.AssetName as Name,
                ISNULL(c.CategoryName, 'Uncategorized') as AdditionalInfo,
                '/api/Asset/' + CAST(a.AssetId AS VARCHAR) + '/photo' as PhotoUrl
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            WHERE a.IsActive = 1
              AND (a.AssetCode LIKE @SearchPattern
                   OR a.AssetName LIKE @SearchPattern)
            ORDER BY 
                CASE 
                    WHEN a.AssetCode LIKE @SearchPattern THEN 1
                    WHEN a.AssetName LIKE @SearchPattern THEN 2
                    ELSE 3
                END,
                a.AssetCode";

        var assets = await _context.QueryAsync<HomeSearchItemDto>(assetSql, new { SearchPattern = searchPattern });
        results.AddRange(assets);

        // ============================================================
        // SEARCH EMPLOYEES
        // ============================================================

        const string employeeSql = @"
            SELECT TOP 10
                'Employee' as Type,
                e.EmployeeId as Id,
                e.EmployeeCode as Code,
                e.FullName as Name,
                ISNULL(md.MasterDataName, '-') as AdditionalInfo,
                '/api/Employee/' + CAST(e.EmployeeId AS VARCHAR) + '/photo' as PhotoUrl
            FROM Employee e
            LEFT JOIN MasterData md ON e.Position = md.ReferenceCode AND md.ReferenceName = 'Position' AND md.IsActive = 1
            WHERE e.IsActive = 1
              AND (e.EmployeeCode LIKE @SearchPattern
                   OR e.FullName LIKE @SearchPattern)
            ORDER BY 
                CASE 
                    WHEN e.EmployeeCode LIKE @SearchPattern THEN 1
                    WHEN e.FullName LIKE @SearchPattern THEN 2
                    ELSE 3
                END,
                e.EmployeeCode";

        var employees = await _context.QueryAsync<HomeSearchItemDto>(employeeSql, new { SearchPattern = searchPattern });
        results.AddRange(employees);

        // Sort results: Assets first, then Employees
        return results.OrderBy(r => r.Type).ThenBy(r => r.Code);
    }
}