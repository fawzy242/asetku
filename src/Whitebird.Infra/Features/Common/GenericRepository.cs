using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Common.Attributes;

namespace Whitebird.Infra.Features.Common;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DapperContext _context;
    private readonly ILogger<GenericRepository<T>> _logger;
    private readonly string _tableName;
    private readonly string _primaryKeyName;
    private readonly List<PropertyInfo> _columnProperties;

    public GenericRepository(DapperContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _logger = logger;
        _tableName = GetTableName();
        _primaryKeyName = GetPrimaryKeyName();
        _columnProperties = GetColumnProperties();
    }

    private string GetTableName()
    {
        var typeName = typeof(T).Name;
        return typeName.EndsWith("Entity", StringComparison.OrdinalIgnoreCase)
            ? typeName[..^6]
            : typeName;
    }

    private string GetPrimaryKeyName()
    {
        var possiblePkNames = new[] { $"{_tableName}Id", "Id", $"{_tableName}_Id" };

        foreach (var pkName in possiblePkNames)
        {
            var pkProperty = typeof(T).GetProperty(pkName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pkProperty != null)
                return pkProperty.Name;
        }

        throw new InvalidOperationException($"Primary key not found for {typeof(T).Name}. Tried: {string.Join(", ", possiblePkNames)}");
    }

    private List<PropertyInfo> GetColumnProperties()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToList();

        var columnProperties = new List<PropertyInfo>();

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                continue;

            var propType = prop.PropertyType;
            if (!propType.IsValueType && propType != typeof(string))
                continue;

            columnProperties.Add(prop);
        }

        return columnProperties;
    }

    private List<PropertyInfo> GetInsertableProperties()
    {
        return _columnProperties
            .Where(p => !p.Name.Equals(_primaryKeyName, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Name != "CreatedDate" && p.Name != "CreatedBy" && p.Name != "ModifiedDate" && p.Name != "ModifiedBy")
            .ToList();
    }

    private List<PropertyInfo> GetUpdateableProperties()
    {
        return _columnProperties
            .Where(p => !p.Name.Equals(_primaryKeyName, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Name != "CreatedDate" && p.Name != "CreatedBy")
            .ToList();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        var query = $"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @id";
        return await _context.QueryFirstOrDefaultAsync<T>(query, new { id });
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM {_tableName} ORDER BY {_primaryKeyName}";
        return await _context.QueryAsync<T>(query);
    }

    public async Task<object> InsertAsync(T entity, IDbTransaction? transaction = null)
    {
        var insertableProps = GetInsertableProperties();
        var columns = string.Join(", ", insertableProps.Select(p => p.Name));
        var values = string.Join(", ", insertableProps.Select(p => $"@{p.Name}"));

        // Buat DynamicParameters dan handle null values
        var parameters = new DynamicParameters();
        foreach (var prop in insertableProps)
        {
            var value = prop.GetValue(entity);
            // Gunakan null, bukan DBNull.Value untuk Dapper
            parameters.Add($"@{prop.Name}", value);
        }

        // Untuk tabel yang memiliki trigger, kita tidak bisa menggunakan OUTPUT INSERTED
        // Alternatif: Insert dulu, lalu ambil ID terakhir dari sequence
        var insertQuery = $"INSERT INTO {_tableName} ({columns}) VALUES ({values});";

        // Execute insert
        await _context.Connection.ExecuteAsync(insertQuery, parameters, transaction);

        // Ambil ID terakhir yang di-insert (untuk SEQUENCE, gunakan query terpisah)
        var getLastIdQuery = $"SELECT CAST(current_value AS INT) FROM sys.sequences WHERE name = 'Seq_{_tableName}Id'";
        var newId = await _context.Connection.ExecuteScalarAsync<object>(getLastIdQuery, transaction: transaction);

        var idProperty = typeof(T).GetProperty(_primaryKeyName);
        if (idProperty != null && newId != null && newId != DBNull.Value)
        {
            idProperty.SetValue(entity, Convert.ChangeType(newId, idProperty.PropertyType));
        }

        if (newId == null || newId == DBNull.Value)
            throw new InvalidOperationException($"Insert failed - no ID returned from {_tableName} table");

        return newId;
    }

    public async Task<int> UpdateAsync(T entity, IDbTransaction? transaction = null)
    {
        var updateableProps = GetUpdateableProperties();
        var setClause = string.Join(", ", updateableProps.Select(p => $"{p.Name} = @{p.Name}"));
        var query = $"UPDATE {_tableName} SET {setClause} WHERE {_primaryKeyName} = @{_primaryKeyName}";

        var parameters = new DynamicParameters();
        foreach (var prop in updateableProps)
        {
            var value = prop.GetValue(entity);
            parameters.Add($"@{prop.Name}", value);
        }

        var pkProperty = typeof(T).GetProperty(_primaryKeyName);
        if (pkProperty != null)
        {
            var pkValue = pkProperty.GetValue(entity);
            parameters.Add($"@{_primaryKeyName}", pkValue);
        }

        return await _context.Connection.ExecuteAsync(query, parameters, transaction);
    }

    public async Task<int> DeleteAsync(object id, IDbTransaction? transaction = null)
    {
        var query = $"DELETE FROM {_tableName} WHERE {_primaryKeyName} = @id";
        return await _context.Connection.ExecuteAsync(query, new { id }, transaction);
    }

    public async Task<int> SoftDeleteAsync(object id, IDbTransaction? transaction = null)
    {
        var query = $"UPDATE {_tableName} SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE {_primaryKeyName} = @id";
        return await _context.Connection.ExecuteAsync(query, new { id, ModifiedDate = DateTime.Now }, transaction);
    }

    public async Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null)
    {
        return await _context.QueryAsync<T>(sql, parameters);
    }

    public async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
    {
        return await _context.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public async Task<PaginatedResult<T>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();

        if (filters?.Any() == true)
        {
            var conditions = new List<string>();
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
            if (conditions.Any())
            {
                whereClause.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            }
        }

        if (_columnProperties.Any(p => p.Name == "IsActive"))
        {
            whereClause.Append(whereClause.Length == 0 ? " WHERE IsActive = 1" : " AND IsActive = 1");
        }

        sortBy ??= _primaryKeyName;
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $"SELECT COUNT(*) FROM {_tableName}{whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = $@"SELECT * FROM {_tableName} {whereClause} ORDER BY {orderBy} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<T>(dataSql, parameters);

        return new PaginatedResult<T>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Filters = filters,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
    }

    public async Task<bool> ExistsAsync(object id)
    {
        var query = $"SELECT COUNT(1) FROM {_tableName} WHERE {_primaryKeyName} = @id";
        var count = await _context.ExecuteScalarAsync<int>(query, new { id });
        return count > 0;
    }

    public async Task<int> CountAsync(Dictionary<string, object>? filters = null)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();

        if (filters?.Any() == true)
        {
            var conditions = new List<string>();
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
            if (conditions.Any())
            {
                whereClause.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            }
        }

        var sql = $"SELECT COUNT(*) FROM {_tableName}{whereClause}";
        return await _context.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<int> BulkInsertAsync(IEnumerable<T> entities)
    {
        var insertableProps = GetInsertableProperties();
        var columns = string.Join(", ", insertableProps.Select(p => p.Name));
        var values = string.Join(", ", insertableProps.Select(p => $"@{p.Name}"));
        var query = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";

        var count = 0;
        foreach (var entity in entities)
        {
            var parameters = new DynamicParameters();
            foreach (var prop in insertableProps)
            {
                var value = prop.GetValue(entity);
                parameters.Add($"@{prop.Name}", value);
            }
            count += await _context.ExecuteAsync(query, parameters);
        }
        return count;
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<T> entities)
    {
        var updateableProps = GetUpdateableProperties();
        var setClause = string.Join(", ", updateableProps.Select(p => $"{p.Name} = @{p.Name}"));
        var query = $"UPDATE {_tableName} SET {setClause} WHERE {_primaryKeyName} = @{_primaryKeyName}";

        var count = 0;
        foreach (var entity in entities)
        {
            var parameters = new DynamicParameters();
            foreach (var prop in updateableProps)
            {
                var value = prop.GetValue(entity);
                parameters.Add($"@{prop.Name}", value);
            }

            var pkProperty = typeof(T).GetProperty(_primaryKeyName);
            if (pkProperty != null)
            {
                var pkValue = pkProperty.GetValue(entity);
                parameters.Add($"@{_primaryKeyName}", pkValue);
            }

            count += await _context.ExecuteAsync(query, parameters);
        }
        return count;
    }

    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, object? parameters = null)
    {
        return await _context.QueryAsync<T>(procedureName, parameters, CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, object? parameters = null)
    {
        return await _context.ExecuteAsync(procedureName, parameters, CommandType.StoredProcedure);
    }
}