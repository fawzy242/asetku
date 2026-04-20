using Dapper;
using System.Data;

namespace Whitebird.Infra.Database;

public class DapperContext : IDisposable, IAsyncDisposable
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public DapperContext(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection Connection
    {
        get
        {
            _connection ??= _connectionFactory.CreateConnection();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public void BeginTransaction()
    {
        _transaction = Connection.BeginTransaction();
    }

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        _transaction = Connection.BeginTransaction(isolationLevel);
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.QueryAsync<T>(command);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.QueryFirstOrDefaultAsync<T>(command);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.ExecuteAsync(command);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.ExecuteScalarAsync<T>(command);
    }

    public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? parameters = null, CommandType? commandType = null)
    {
        var command = new CommandDefinition(sql, parameters, _transaction, commandType: commandType);
        return await Connection.QueryMultipleAsync(command);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            if (_connection is IAsyncDisposable asyncConnection)
            {
                await asyncConnection.DisposeAsync();
            }
            else
            {
                _connection?.Dispose();
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}