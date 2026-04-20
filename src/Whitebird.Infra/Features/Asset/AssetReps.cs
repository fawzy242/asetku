using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

public class AssetReps : IAssetReps
{
    private readonly DapperContext _context;

    public AssetReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<AssetEntity?> GetByIdRawAsync(int assetId)
    {
        const string sql = "SELECT * FROM Asset WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetEntity>(sql, new { AssetId = assetId });
    }

    public async Task<AssetEntity?> GetByIdWithRelationsAsync(int assetId)
    {
        const string sql = @"
            SELECT a.*, 
                   c.CategoryName, 
                   e.FullName as CurrentHolderName, 
                   s.SupplierName, 
                   r.FullName as ResponsiblePartyName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            LEFT JOIN Employee r ON a.ResponsiblePartyId = r.EmployeeId
            WHERE a.AssetId = @AssetId AND a.IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<AssetEntity>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<AssetEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT a.*, 
                   c.CategoryName, 
                   e.FullName as CurrentHolderName, 
                   s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.IsActive = 1
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetEntity>(sql);
    }

    public async Task<IEnumerable<AssetEntity>> GetByCategoryWithRelationsAsync(int categoryId)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.CategoryId = @CategoryId AND a.IsActive = 1
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetEntity>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<AssetEntity>> GetByStatusWithRelationsAsync(string status)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.Status = @Status AND a.IsActive = 1
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetEntity>(sql, new { Status = status });
    }

    public async Task<IEnumerable<AssetEntity>> GetByHolderWithRelationsAsync(int employeeId)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.CurrentHolderId = @EmployeeId AND a.IsActive = 1
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<AssetEntity>> GetExpiredWarrantyWithRelationsAsync()
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.WarrantyExpiryDate < GETDATE() AND a.WarrantyExpiryDate IS NOT NULL AND a.IsActive = 1
            ORDER BY a.WarrantyExpiryDate";

        return await _context.QueryAsync<AssetEntity>(sql);
    }

    public async Task<IEnumerable<AssetEntity>> GetUpcomingMaintenanceWithRelationsAsync(int daysAhead = 30)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE a.NextMaintenanceDate BETWEEN GETDATE() AND DATEADD(DAY, @DaysAhead, GETDATE()) AND a.IsActive = 1
            ORDER BY a.NextMaintenanceDate";

        return await _context.QueryAsync<AssetEntity>(sql, new { DaysAhead = daysAhead });
    }

    public async Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null)
    {
        var sql = "SELECT COUNT(1) FROM Asset WHERE AssetCode = @AssetCode AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@AssetCode", assetCode);

        if (excludeAssetId.HasValue)
        {
            sql += " AND AssetId != @ExcludeAssetId";
            parameters.Add("@ExcludeAssetId", excludeAssetId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetNextAssetNumberAsync()
    {
        const string sql = @"SELECT ISNULL(MAX(CAST(SUBSTRING(AssetCode, 5, LEN(AssetCode)) AS INT)), 0) + 1 
                             FROM Asset WHERE AssetCode LIKE 'AST-%' AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<PaginatedResult<AssetEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        var conditions = new List<string> { "a.IsActive = 1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR a.Brand LIKE @Search OR a.Model LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"a.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = string.Join(" AND ", conditions);
        sortBy = string.IsNullOrEmpty(sortBy) ? "a.AssetCode" : $"a.{sortBy}";
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"SELECT COUNT(*) FROM Asset a WHERE {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT a.*, c.CategoryName, e.FullName as CurrentHolderName, s.SupplierName
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            WHERE {whereClause}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetEntity>(dataSql, parameters);

        return new PaginatedResult<AssetEntity>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}