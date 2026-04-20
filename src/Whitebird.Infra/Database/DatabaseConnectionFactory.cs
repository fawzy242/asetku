using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Whitebird.Infra.Database;

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _connectionStrings;

    public DatabaseConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionStrings = new Dictionary<string, string>();

        var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
        foreach (var child in connectionStringsSection.GetChildren())
        {
            _connectionStrings[child.Key] = child.Value ?? string.Empty;
        }
    }

    public IDbConnection CreateConnection()
    {
        return CreateConnection("DefaultConnection");
    }

    public IDbConnection CreateConnection(string connectionStringName)
    {
        if (!_connectionStrings.TryGetValue(connectionStringName, out var connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found");
        }

        return new SqlConnection(connectionString);
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        return await Task.FromResult(CreateConnection());
    }

    public string GetConnectionString(string name = "DefaultConnection")
    {
        return _connectionStrings.GetValueOrDefault(name) ?? string.Empty;
    }
}