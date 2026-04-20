using System.Data;

namespace Whitebird.Infra.Database;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
    IDbConnection CreateConnection(string connectionStringName);
    Task<IDbConnection> CreateConnectionAsync();
    string GetConnectionString(string name = "DefaultConnection");
}