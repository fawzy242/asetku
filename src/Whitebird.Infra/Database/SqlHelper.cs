using Dapper;
using System.Data;

namespace Whitebird.Infra.Database;

public class SqlHelper
{
    private readonly DapperContext _context;

    public SqlHelper(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procedureName, object? parameters = null)
    {
        return await _context.QueryAsync<T>(procedureName, parameters, CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, object? parameters = null)
    {
        return await _context.ExecuteAsync(procedureName, parameters, CommandType.StoredProcedure);
    }

    public async Task<T?> ExecuteStoredProcedureScalarAsync<T>(string procedureName, object? parameters = null)
    {
        return await _context.ExecuteScalarAsync<T>(procedureName, parameters, CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        return await _context.QueryAsync<T>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        return await _context.ExecuteAsync(sql, parameters);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        return await _context.ExecuteScalarAsync<T>(sql, parameters);
    }
}