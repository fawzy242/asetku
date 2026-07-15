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
            WHERE s.IsActive = 1
            ORDER BY s.SupplierName";
        
        return await _context.QueryAsync<SupplierListView>(sql);
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<SupplierListView>> GetPagedListAsync(SupplierGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // IsActive filter
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "s.IsActive", request.IsActive);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "s.SupplierName", "s.ContactPerson", "s.Email", "s.PhoneNumber", "s.Address" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "s.SupplierName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        // Count
        var countSql = $@"
            SELECT COUNT(*) 
            FROM Supplier s
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var selectClause = @"
            SELECT 
                s.SupplierId, s.SupplierName, s.ContactPerson,
                s.PhoneNumber, s.Email, s.Address,
                s.IsActive, s.CreatedDate, s.CreatedBy, s.ModifiedDate, s.ModifiedBy,
                (SELECT COUNT(*) FROM Asset WHERE SupplierId = s.SupplierId AND IsActive = 1) as AssetCount";

        var dataSql = $@"
            {selectClause}
            FROM Supplier s
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<SupplierListView>(dataSql, parameters);

        return new PaginatedResult<SupplierListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

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