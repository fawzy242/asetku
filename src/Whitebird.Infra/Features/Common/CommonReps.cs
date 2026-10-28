using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Whitebird.Infra.Database;

namespace Whitebird.Infra.Features.Common;

/// <summary>
/// Single implementation of <see cref="ICommonReps"/> using Dapper.
/// Behavioural parity with the previous <c>GenericRepository&lt;T&gt;</c>:
/// same table/PK inference, same insert/update column filtering, same
/// OUTPUT INSERTED + SCOPE_IDENTITY fallback, same SqlBulkCopy with per-row fallback.
///
/// Only two intentional differences:
/// * Method-level generics with a <b>static per-type metadata cache</b> instead of
///   per-instance cache — the reflection cost is amortized once per entity type
///   across the process, not once per HTTP request.
/// * <see cref="SoftDeleteAsync{TEntity}"/> records <c>ModifiedBy</c> in addition to
///   <c>ModifiedDate</c> (the previous implementation only set the date).
/// </summary>
public class CommonReps : ICommonReps
{
    private readonly DapperContext _context;
    private readonly ILogger<CommonReps> _logger;

    // Per-type reflection cache — computed once per entity type across the process.
    private static readonly ConcurrentDictionary<Type, EntityMetadata> _metadataCache = new();

    public CommonReps(DapperContext context, ILogger<CommonReps> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // METADATA
    // ============================================================

    private sealed class EntityMetadata
    {
        public string TableName { get; init; } = string.Empty;
        public string PrimaryKeyName { get; init; } = string.Empty;
        public List<PropertyInfo> InsertableProperties { get; init; } = new();
        public List<PropertyInfo> UpdateableProperties { get; init; } = new();
    }

    private static EntityMetadata GetMetadata<TEntity>() where TEntity : class
    {
        return _metadataCache.GetOrAdd(typeof(TEntity), _ => BuildMetadata<TEntity>());
    }

    private static EntityMetadata BuildMetadata<TEntity>() where TEntity : class
    {
        var typeName = typeof(TEntity).Name;
        var tableName = typeName.EndsWith("Entity", StringComparison.OrdinalIgnoreCase)
            ? typeName[..^6]
            : typeName;

        var pkCandidates = new[] { $"{tableName}Id", "Id", $"{tableName}_Id" };
        string? pkName = null;
        foreach (var candidate in pkCandidates)
        {
            if (typeof(TEntity).GetProperty(candidate, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null)
            {
                pkName = candidate;
                break;
            }
        }
        if (pkName == null)
        {
            throw new InvalidOperationException(
                $"Primary key not found for {typeof(TEntity).Name}. Tried: {string.Join(", ", pkCandidates)}");
        }

        // Only scalar / string / DateTime properties are treated as columns.
        var columnProps = new List<PropertyInfo>();
        foreach (var prop in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            var pt = prop.PropertyType;
            if (!pt.IsValueType && pt != typeof(string) && pt != typeof(DateTime) && pt != typeof(DateTime?)) continue;
            var underlying = Nullable.GetUnderlyingType(pt);
            if (underlying != null && !underlying.IsValueType) continue;
            columnProps.Add(prop);
        }

        var insertable = columnProps
            .Where(p => !p.Name.Equals(pkName, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Name != "CreatedDate" && p.Name != "CreatedBy"
                     && p.Name != "ModifiedDate" && p.Name != "ModifiedBy")
            .ToList();

        var updateable = columnProps
            .Where(p => !p.Name.Equals(pkName, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Name != "CreatedDate" && p.Name != "CreatedBy")
            .ToList();

        return new EntityMetadata
        {
            TableName = tableName,
            PrimaryKeyName = pkName,
            InsertableProperties = insertable,
            UpdateableProperties = updateable
        };
    }

    // ============================================================
    // FETCH
    // ============================================================

    public async Task<TEntity?> GetByIdAsync<TEntity>(object id) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var sql = $"SELECT * FROM {md.TableName} WHERE {md.PrimaryKeyName} = @id";
        return await _context.QueryFirstOrDefaultAsync<TEntity>(sql, new { id });
    }

    // ============================================================
    // INSERT
    // ============================================================

    public async Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var columns = string.Join(", ", md.InsertableProperties.Select(p => p.Name));
        var values = string.Join(", ", md.InsertableProperties.Select(p => $"@{p.Name}"));

        var parameters = new DynamicParameters();
        foreach (var prop in md.InsertableProperties)
        {
            parameters.Add($"@{prop.Name}", prop.GetValue(entity));
        }

        try
        {
            var insertQuery = $@"
                INSERT INTO {md.TableName} ({columns})
                OUTPUT INSERTED.{md.PrimaryKeyName}
                VALUES ({values});";

            var newId = await _context.Connection.ExecuteScalarAsync<object>(insertQuery, parameters, transaction);
            SetIdentity(entity, md.PrimaryKeyName, newId);

            if (newId == null || newId == DBNull.Value)
                throw new InvalidOperationException($"Insert failed - no ID returned from {md.TableName} table");

            return newId;
        }
        catch (Exception ex) when (ex.Message.Contains("OUTPUT") || ex.Message.Contains("trigger"))
        {
            _logger.LogWarning(ex, "OUTPUT clause failed for {TableName}, falling back to INSERT + SCOPE_IDENTITY", md.TableName);

            // Fallback for tables with triggers: OUTPUT INSERTED is not supported
            // when a trigger is present. SCOPE_IDENTITY() must run in the same batch
            // as the INSERT so it returns the identity produced by this statement.
            var insertQuery = $@"
                INSERT INTO {md.TableName} ({columns}) VALUES ({values});
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var newId = await _context.Connection.ExecuteScalarAsync<object>(insertQuery, parameters, transaction);
            SetIdentity(entity, md.PrimaryKeyName, newId);

            if (newId == null || newId == DBNull.Value)
                throw new InvalidOperationException($"Insert failed - no ID returned from {md.TableName} table");

            return newId;
        }
    }

    private static void SetIdentity<TEntity>(TEntity entity, string pkName, object? newId) where TEntity : class
    {
        if (newId == null || newId == DBNull.Value) return;
        var idProp = typeof(TEntity).GetProperty(pkName);
        if (idProp != null)
        {
            idProp.SetValue(entity, Convert.ChangeType(newId, idProp.PropertyType));
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public async Task<int> UpdateAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var setClause = string.Join(", ", md.UpdateableProperties.Select(p => $"{p.Name} = @{p.Name}"));
        var sql = $"UPDATE {md.TableName} SET {setClause} WHERE {md.PrimaryKeyName} = @{md.PrimaryKeyName}";

        var parameters = new DynamicParameters();
        foreach (var prop in md.UpdateableProperties)
        {
            parameters.Add($"@{prop.Name}", prop.GetValue(entity));
        }

        var pkProp = typeof(TEntity).GetProperty(md.PrimaryKeyName);
        if (pkProp != null)
        {
            parameters.Add($"@{md.PrimaryKeyName}", pkProp.GetValue(entity));
        }

        return await _context.Connection.ExecuteAsync(sql, parameters, transaction);
    }

    // ============================================================
    // DELETE
    // ============================================================

    public async Task<int> DeleteAsync<TEntity>(object id, IDbTransaction? transaction = null) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var sql = $"DELETE FROM {md.TableName} WHERE {md.PrimaryKeyName} = @id";
        return await _context.Connection.ExecuteAsync(sql, new { id }, transaction);
    }

    public async Task<int> SoftDeleteAsync<TEntity>(object id, string modifiedBy) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var sql = $@"
            UPDATE {md.TableName}
            SET IsActive = 0, ModifiedDate = GETDATE(), ModifiedBy = @ModifiedBy
            WHERE {md.PrimaryKeyName} = @id";
        return await _context.Connection.ExecuteAsync(sql, new { id, ModifiedBy = modifiedBy });
    }

    // ============================================================
    // BULK INSERT (SqlBulkCopy with per-row fallback)
    // ============================================================

    public async Task<int> BulkInsertOptimizedAsync<TEntity>(IEnumerable<TEntity> entities, IDbTransaction? transaction = null) where TEntity : class
    {
        var entityList = entities.ToList();
        if (!entityList.Any()) return 0;

        var md = GetMetadata<TEntity>();
        var dataTable = new DataTable();

        foreach (var prop in md.InsertableProperties)
        {
            var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            dataTable.Columns.Add(prop.Name, columnType);
        }

        foreach (var entity in entityList)
        {
            var row = dataTable.NewRow();
            foreach (var prop in md.InsertableProperties)
            {
                var value = prop.GetValue(entity);
                row[prop.Name] = value ?? DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }

        // SqlBulkCopy requires an explicit SqlTransaction. When a scope-based
        // TransactionScope is in play (the common case), no explicit transaction is
        // available on the context, so we fall back to per-row inserts — which stay
        // inside the ambient transaction. Same policy as before.
        try
        {
            if (_context.Connection is SqlConnection connection && transaction is SqlTransaction sqlTx)
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, sqlTx)
                {
                    DestinationTableName = md.TableName,
                    BatchSize = 1000,
                    BulkCopyTimeout = 300
                };
                await bulkCopy.WriteToServerAsync(dataTable);
                return entityList.Count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SqlBulkCopy failed for {TableName}, falling back to per-row insert", md.TableName);
        }

        return await BulkInsertAsync(entityList);
    }

    private async Task<int> BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        var md = GetMetadata<TEntity>();
        var columns = string.Join(", ", md.InsertableProperties.Select(p => p.Name));
        var values = string.Join(", ", md.InsertableProperties.Select(p => $"@{p.Name}"));
        var query = $"INSERT INTO {md.TableName} ({columns}) VALUES ({values})";

        var count = 0;
        foreach (var entity in entities)
        {
            var parameters = new DynamicParameters();
            foreach (var prop in md.InsertableProperties)
            {
                parameters.Add($"@{prop.Name}", prop.GetValue(entity));
            }
            count += await _context.ExecuteAsync(query, parameters);
        }
        return count;
    }
}