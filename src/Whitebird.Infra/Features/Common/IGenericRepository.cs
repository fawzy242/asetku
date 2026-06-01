using System.Data;

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
    /// <param name="id">Primary key value</param>
    /// <returns>Entity instance or null if not found</returns>
    Task<T?> GetByIdAsync(object id);
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Inserts a new entity
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="transaction">Optional transaction</param>
    /// <returns>Generated primary key value</returns>
    Task<object> InsertAsync(T entity, IDbTransaction? transaction = null);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity with updated values</param>
    /// <param name="transaction">Optional transaction</param>
    /// <returns>Number of rows affected</returns>
    Task<int> UpdateAsync(T entity, IDbTransaction? transaction = null);
    
    /// <summary>
    /// Permanently deletes an entity
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="transaction">Optional transaction</param>
    /// <returns>Number of rows affected</returns>
    Task<int> DeleteAsync(object id, IDbTransaction? transaction = null);
    
    /// <summary>
    /// Soft deletes an entity (sets IsActive = false)
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="transaction">Optional transaction</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SoftDeleteAsync(object id, IDbTransaction? transaction = null);
    
    /// <summary>
    /// Executes a raw SQL query and maps results to entities
    /// </summary>
    /// <param name="sql">SQL query string</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null);
    
    /// <summary>
    /// Executes a raw SQL query and returns first result or default
    /// </summary>
    /// <param name="sql">SQL query string</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Entity or default</returns>
    Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null);
    
    /// <summary>
    /// Gets paginated list of entities
    /// </summary>
    /// <param name="pageNumber">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters</param>
    /// <returns>Paginated result</returns>
    Task<PaginatedResult<T>> GetPagedAsync(
        int pageNumber = 1, 
        int pageSize = 10, 
        string? sortBy = null, 
        bool sortDescending = false, 
        Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(object id);
    
    /// <summary>
    /// Counts entities with optional filters
    /// </summary>
    /// <param name="filters">Optional filters</param>
    /// <returns>Count of entities</returns>
    Task<int> CountAsync(Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Bulk inserts multiple entities (regular Dapper bulk insert)
    /// </summary>
    /// <param name="entities">Entities to insert</param>
    /// <returns>Number of rows inserted</returns>
    Task<int> BulkInsertAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Bulk updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <returns>Number of rows updated</returns>
    Task<int> BulkUpdateAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Executes a stored procedure and returns entities
    /// </summary>
    /// <param name="procedureName">Stored procedure name</param>
    /// <param name="parameters">Procedure parameters</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, object? parameters = null);
    
    /// <summary>
    /// Executes a stored procedure that doesn't return data
    /// </summary>
    /// <param name="procedureName">Stored procedure name</param>
    /// <param name="parameters">Procedure parameters</param>
    /// <returns>Number of rows affected</returns>
    Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, object? parameters = null);
    
    /// <summary>
    /// Optimized bulk insert using SqlBulkCopy (much faster for large datasets)
    /// </summary>
    /// <param name="entities">Entities to insert</param>
    /// <param name="transaction">Optional transaction</param>
    /// <returns>Number of rows inserted</returns>
    Task<int> BulkInsertOptimizedAsync(IEnumerable<T> entities, IDbTransaction? transaction = null);
}