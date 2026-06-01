using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Supplier;

/// <summary>
/// Repository implementation for Supplier operations using Dapper
/// </summary>
public class SupplierReps : ISupplierReps
{
    private readonly DapperContext _context;

    public SupplierReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<SupplierEntity?> GetByIdRawAsync(int supplierId)
    {
        const string sql = "SELECT * FROM Supplier WHERE SupplierId = @SupplierId";
        return await _context.QueryFirstOrDefaultAsync<SupplierEntity>(sql, new { SupplierId = supplierId });
    }

    public async Task<bool> IsSupplierNameExistsAsync(string supplierName, int? excludeSupplierId = null)
    {
        var sql = "SELECT COUNT(1) FROM Supplier WHERE SupplierName = @SupplierName";
        var parameters = new DynamicParameters();
        parameters.Add("@SupplierName", supplierName);

        if (excludeSupplierId.HasValue)
        {
            sql += " AND SupplierId != @ExcludeSupplierId";
            parameters.Add("@ExcludeSupplierId", excludeSupplierId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetAssetCountAsync(int supplierId)
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE SupplierId = @SupplierId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { SupplierId = supplierId });
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<SupplierDetailView?> GetDetailByIdAsync(int supplierId)
    {
        const string sql = @"
            SELECT 
                s.SupplierId, s.SupplierName, s.ContactPerson,
                s.PhoneNumber, s.Email, s.Address,
                s.IsActive, s.CreatedDate, s.CreatedBy, s.ModifiedDate, s.ModifiedBy,
                (SELECT COUNT(*) FROM Asset WHERE SupplierId = s.SupplierId AND IsActive = 1) as AssetCount
            FROM Supplier s
            WHERE s.SupplierId = @SupplierId";
        
        return await _context.QueryFirstOrDefaultAsync<SupplierDetailView>(sql, new { SupplierId = supplierId });
    }

    public async Task<IEnumerable<SupplierListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                s.SupplierId, s.SupplierName, s.ContactPerson,
                s.PhoneNumber, s.Email, s.Address,
                s.IsActive, s.CreatedDate, s.CreatedBy, s.ModifiedDate, s.ModifiedBy,
                (SELECT COUNT(*) FROM Asset WHERE SupplierId = s.SupplierId AND IsActive = 1) as AssetCount
            FROM Supplier s
            ORDER BY s.SupplierName";
        
        return await _context.QueryAsync<SupplierListView>(sql);
    }

    public async Task<IEnumerable<SupplierListView>> GetActiveOnlyListViewAsync()
    {
        const string sql = @"
            SELECT 
                s.SupplierId, s.SupplierName, s.ContactPerson,
                s.PhoneNumber, s.Email, s.Address,
                s.IsActive, s.CreatedDate, s.CreatedBy, s.ModifiedDate, s.ModifiedBy,
                (SELECT COUNT(*) FROM Asset WHERE SupplierId = s.SupplierId AND IsActive = 1) as AssetCount
            FROM Supplier s
            WHERE s.IsActive = 1
            ORDER BY s.SupplierName";
        
        return await _context.QueryAsync<SupplierListView>(sql);
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    private string BuildBaseSupplierQueryWithPagination(string selectClause, string whereClause, string orderBy, int offset, int pageSize)
    {
        return $@"
            {selectClause}
            FROM Supplier s
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";
    }

    private string BuildSupplierListViewSelectClause()
    {
        return @"
            SELECT 
                s.SupplierId,
                s.SupplierName,
                s.ContactPerson,
                s.PhoneNumber,
                s.Email,
                s.Address,
                s.IsActive,
                s.CreatedDate,
                s.CreatedBy,
                s.ModifiedDate,
                s.ModifiedBy,
                (SELECT COUNT(*) FROM Asset WHERE SupplierId = s.SupplierId AND IsActive = 1) as AssetCount";
    }

    private (string WhereClause, DynamicParameters Parameters) BuildSupplierWhereClause(
        string? search = null,
        bool? isActive = null,
        Dictionary<string, object>? additionalFilters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Handle isActive filter
        if (isActive.HasValue)
        {
            conditions.Add($"s.IsActive = {(isActive.Value ? 1 : 0)}");
        }
        // No default - show all suppliers if not specified

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(s.SupplierName LIKE @Search OR s.ContactPerson LIKE @Search OR s.Email LIKE @Search OR s.PhoneNumber LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (additionalFilters != null)
        {
            foreach (var filter in additionalFilters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"s.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        return (whereClause, parameters);
    }

    public async Task<PaginatedResult<SupplierListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        bool? isActiveFilter = null;

        if (filters != null)
        {
            if (filters.ContainsKey("isActive") && bool.TryParse(filters["isActive"]?.ToString(), out bool isActive))
            {
                isActiveFilter = isActive;
            }
            filters?.Remove("isActive");
        }

        var (whereClause, parameters) = BuildSupplierWhereClause(search, isActiveFilter, filters);

        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "s.SupplierName";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("s."))
            {
                sortBy = $"s.{sortBy}";
            }
        }
        
        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Supplier s
            {whereClause}";
        
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildSupplierListViewSelectClause();
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseSupplierQueryWithPagination(selectClause, whereClause, orderBy, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<SupplierListView>(dataSql, parameters);

        return new PaginatedResult<SupplierListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Filters = filters,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
    }

    public async Task<IEnumerable<SupplierDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT SupplierId, SupplierName, ContactPerson
            FROM Supplier
            WHERE IsActive = 1
            ORDER BY SupplierName";

        return await _context.QueryAsync<SupplierDropdownView>(sql);
    }
}