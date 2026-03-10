using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace PCautivoCore.Infrastructure.Persistence;

public class MssqlContext(IConfiguration configuration)
{
    private readonly string? _defaultConnection = configuration.GetConnectionString("DefaultConnection");

    public IDbConnection CreateDefaultConnection()
    {
        return new SqlConnection(_defaultConnection);
    }
}