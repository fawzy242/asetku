using System.Data;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Infra.Features.Common;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type (must be a class)</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its primary key
    /// </summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Inserts a new entity
    /// </summary>
    Task<object> InsertAsync(T entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task<int> UpdateAsync(T entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Permanently deletes an entity
    /// </summary>
    Task<int> DeleteAsync(object id, IDbTransaction? transaction = null);

    /// <summary>
    /// Soft deletes an entity (sets IsActive = false)
    /// </summary>
    Task<int> SoftDeleteAsync(object id, IDbTransaction? transaction = null);

    /// <summary>
    /// Executes a raw SQL query and maps results to entities
    /// </summary>
    Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null);

    /// <summary>
    /// Executes a raw SQL query and returns first result or default
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null);

    /// <summary>
    /// Gets paginated list of entities
    /// </summary>
    Task<PaginatedResult<T>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, object>? filters = null);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    Task<bool> ExistsAsync(object id);

    /// <summary>
    /// Counts entities with optional filters
    /// </summary>
    Task<int> CountAsync(Dictionary<string, object>? filters = null);

    /// <summary>
    /// Bulk inserts multiple entities (regular Dapper bulk insert)
    /// </summary>
    Task<int> BulkInsertAsync(IEnumerable<T> entities);

    /// <summary>
    /// Bulk updates multiple entities
    /// </summary>
    Task<int> BulkUpdateAsync(IEnumerable<T> entities);

    /// <summary>
    /// Optimized bulk insert using SqlBulkCopy (much faster for large datasets)
    /// </summary>
    Task<int> BulkInsertOptimizedAsync(IEnumerable<T> entities, IDbTransaction? transaction = null);
}