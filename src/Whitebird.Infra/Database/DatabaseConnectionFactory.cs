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
            var value = child.Value ?? string.Empty;

            // Expand environment variables in connection string
            if (value.Contains("%"))
            {
                value = Environment.ExpandEnvironmentVariables(value);
            }

            _connectionStrings[child.Key] = value;
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

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' is empty");
        }

        // Ensure encryption is enabled for production
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        if (isProduction && !connectionString.Contains("Encrypt=True", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Production connection must have Encrypt=True");
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