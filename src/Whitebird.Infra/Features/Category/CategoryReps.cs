using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Category;

namespace Whitebird.Infra.Features.Category;

public class CategoryReps : ICategoryReps
{
    private readonly DapperContext _context;

    public CategoryReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<CategoryEntity?> GetByIdRawAsync(int categoryId)
    {
        const string sql = "SELECT * FROM Category WHERE CategoryId = @CategoryId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<CategoryEntity>(sql, new { CategoryId = categoryId });
    }

    public async Task<CategoryEntity?> GetByIdWithRelationsAsync(int categoryId)
    {
        const string sql = @"
            SELECT c.*, p.CategoryName as ParentCategoryName
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.CategoryId = @CategoryId AND c.IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<CategoryEntity>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<CategoryEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT c.*, p.CategoryName as ParentCategoryName,
                   (SELECT COUNT(*) FROM Category WHERE ParentCategoryId = c.CategoryId AND IsActive = 1) as ChildCount
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.IsActive = 1
            ORDER BY c.CategoryName";
        return await _context.QueryAsync<CategoryEntity>(sql);
    }

    public async Task<IEnumerable<CategoryEntity>> GetActiveOnlyWithRelationsAsync()
    {
        const string sql = @"
            SELECT c.*, p.CategoryName as ParentCategoryName
            FROM Category c
            LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
            WHERE c.IsActive = 1
            ORDER BY c.CategoryName";
        return await _context.QueryAsync<CategoryEntity>(sql);
    }

    public async Task<IEnumerable<CategoryEntity>> GetSubCategoryAsync(int parentCategoryId)
    {
        const string sql = "SELECT * FROM Category WHERE ParentCategoryId = @ParentCategoryId AND IsActive = 1 ORDER BY CategoryName";
        return await _context.QueryAsync<CategoryEntity>(sql, new { ParentCategoryId = parentCategoryId });
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
}