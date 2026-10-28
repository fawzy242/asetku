using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Infra.Features.Category;

/// <summary>
/// Repository implementation for Category operations using Dapper
/// </summary>
public class CategoryReps : ICategoryReps
{
    private readonly DapperContext _context;

    public CategoryReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<CategoryEntity?> GetByIdRawAsync(int categoryId)
    {
        const string sql = "SELECT * FROM Category WHERE CategoryId = @CategoryId";
        return await _context.QueryFirstOrDefaultAsync<CategoryEntity>(sql, new { CategoryId = categoryId });
    }

    public async Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null)
    {
        var sql = "SELECT COUNT(1) FROM Category WHERE CategoryName = @CategoryName AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@CategoryName", categoryName);

        if (excludeCategoryId.HasValue)
        {
            sql += " AND CategoryId != @ExcludeCategoryId";
            parameters.Add("@ExcludeCategoryId", excludeCategoryId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
            return false;

        var sql = "SELECT COUNT(1) FROM Category WHERE CategoryCode = @CategoryCode AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@CategoryCode", categoryCode);

        if (excludeCategoryId.HasValue)
        {
            sql += " AND CategoryId != @ExcludeCategoryId";
            parameters.Add("@ExcludeCategoryId", excludeCategoryId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetChildCountAsync(int categoryId)
    {
        const string sql = "SELECT COUNT(*) FROM Category WHERE ParentCategoryId = @CategoryId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
    }

    public async Task<int> GetAssetCountAsync(int categoryId)
    {
        // No IsActive filter: the FK on Asset.CategoryId fires for any Asset row,
        // active or inactive, so hard-delete must be refused in either case.
        const string sql = "SELECT COUNT(*) FROM Asset WHERE CategoryId = @CategoryId";
        return await _context.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<CategoryDetailView?> GetDetailByIdAsync(int categoryId)
    {
        // No IsActive filter: detail-by-id must return the record in any state.
        const string sql = @"
            SELECT 
                c.CategoryId, c.CategoryCode, c.CategoryName, c.Description,
                c.ParentCategoryId, p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive, c.CreatedDate, c.CreatedBy, c.ModifiedDate, c.ModifiedBy
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.CategoryId = @CategoryId";

        return await _context.QueryFirstOrDefaultAsync<CategoryDetailView>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<CategoryListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                c.CategoryId, c.CategoryCode, c.CategoryName, c.Description,
                c.ParentCategoryId, p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive, c.CreatedDate, c.CreatedBy, c.ModifiedDate, c.ModifiedBy
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.IsActive = 1
            ORDER BY c.CategoryName";

        return await _context.QueryAsync<CategoryListView>(sql);
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<CategoryListView>> GetPagedListAsync(CategoryGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "c.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "c.CategoryName", "c.CategoryCode", "c.Description" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        if (request.ParentCategoryId.HasValue)
        {
            if (request.ParentCategoryId.Value == 0)
            {
                conditions.Add("c.ParentCategoryId IS NULL");
            }
            else
            {
                conditions.Add("c.ParentCategoryId = @ParentCategoryId");
                parameters.Add("@ParentCategoryId", request.ParentCategoryId.Value);
            }
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "c.CategoryName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Category c
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                c.CategoryId, c.CategoryCode, c.CategoryName, c.Description,
                c.ParentCategoryId, p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive, c.CreatedDate, c.CreatedBy, c.ModifiedDate, c.ModifiedBy,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<CategoryListView>(dataSql, parameters);

        return new PaginatedResult<CategoryListView>
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

    public async Task<IEnumerable<CategoryDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT CategoryId, CategoryName, CategoryCode, ParentCategoryId
            FROM Category
            WHERE IsActive = 1
            ORDER BY CategoryName";

        return await _context.QueryAsync<CategoryDropdownView>(sql);
    }
}