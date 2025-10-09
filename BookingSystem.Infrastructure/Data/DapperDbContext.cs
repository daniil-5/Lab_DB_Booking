
using System.Data;
using Npgsql;

namespace BookingSystem.Infrastructure.Data;

public class DapperDbContext
{
    private readonly string _connectionString;

    public DapperDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
