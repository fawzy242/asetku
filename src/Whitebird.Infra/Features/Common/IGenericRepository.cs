using System.Data;

namespace Whitebird.Infra.Features.Common;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<object> InsertAsync(T entity, IDbTransaction? transaction = null);
    Task<int> UpdateAsync(T entity, IDbTransaction? transaction = null);
    Task<int> DeleteAsync(object id, IDbTransaction? transaction = null);
    Task<int> SoftDeleteAsync(object id, IDbTransaction? transaction = null);
    Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null);
    Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null);
    Task<PaginatedResult<T>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);
    Task<bool> ExistsAsync(object id);
    Task<int> CountAsync(Dictionary<string, object>? filters = null);
    Task<int> BulkInsertAsync(IEnumerable<T> entities);
    Task<int> BulkUpdateAsync(IEnumerable<T> entities);
    Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, object? parameters = null);
    Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, object? parameters = null);
}