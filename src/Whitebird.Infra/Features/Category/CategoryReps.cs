using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Category;
using Whitebird.Infra.Features.Common;

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
        const string sql = "SELECT * FROM Category WHERE CategoryId = @CategoryId AND IsActive = 1";
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

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<CategoryDetailView?> GetDetailByIdAsync(int categoryId)
    {
        const string sql = @"
            SELECT 
                c.CategoryId, c.CategoryCode, c.CategoryName, c.Description,
                c.ParentCategoryId, p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive, c.CreatedDate, c.CreatedBy, c.ModifiedDate, c.ModifiedBy
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.CategoryId = @CategoryId AND c.IsActive = 1";
        
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

    public async Task<IEnumerable<CategoryListView>> GetActiveOnlyListViewAsync()
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

    public async Task<IEnumerable<CategoryListView>> GetSubCategoryListViewAsync(int parentCategoryId)
    {
        const string sql = @"
            SELECT 
                c.CategoryId, c.CategoryCode, c.CategoryName, c.Description,
                c.ParentCategoryId, p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive, c.CreatedDate, c.CreatedBy, c.ModifiedDate, c.ModifiedBy
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.ParentCategoryId = @ParentCategoryId AND c.IsActive = 1
            ORDER BY c.CategoryName";
        
        return await _context.QueryAsync<CategoryListView>(sql, new { ParentCategoryId = parentCategoryId });
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    private string BuildBaseCategoryQueryWithPagination(string selectClause, string whereClause, string orderBy, int offset, int pageSize)
    {
        return $@"
            {selectClause}
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";
    }

    private string BuildCategoryListViewSelectClause()
    {
        return @"
            SELECT 
                c.CategoryId,
                c.CategoryCode,
                c.CategoryName,
                c.Description,
                c.ParentCategoryId,
                p.CategoryName as ParentCategoryName,
                (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount,
                c.IsActive,
                c.CreatedDate,
                c.CreatedBy,
                c.ModifiedDate,
                c.ModifiedBy";
    }

    private (string WhereClause, DynamicParameters Parameters) BuildCategoryWhereClause(
        string? search = null,
        bool? isActive = null,
        Dictionary<string, object>? additionalFilters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Handle isActive filter
        if (isActive.HasValue)
        {
            conditions.Add($"c.IsActive = {(isActive.Value ? 1 : 0)}");
        }
        // No default - show all categories if not specified

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(c.CategoryName LIKE @Search OR c.CategoryCode LIKE @Search OR c.Description LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (additionalFilters != null)
        {
            foreach (var filter in additionalFilters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"c.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        return (whereClause, parameters);
    }

    public async Task<PaginatedResult<CategoryListView>> GetPagedListAsync(
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

        var (whereClause, parameters) = BuildCategoryWhereClause(search, isActiveFilter, filters);

        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "c.CategoryName";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("c.") && !sortBy.StartsWith("p."))
            {
                sortBy = $"c.{sortBy}";
            }
        }
        
        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Category c
            {whereClause}";
        
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildCategoryListViewSelectClause();
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseCategoryQueryWithPagination(selectClause, whereClause, orderBy, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<CategoryListView>(dataSql, parameters);

        return new PaginatedResult<CategoryListView>
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