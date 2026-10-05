using System.Data.Common;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace AcademiaDoZe.Infrastructure;

public static class DbProvider
{
    public static DbConnection GetConnection(DatabaseType databaseType, string connectionString)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => new SqlConnection(connectionString),
            DatabaseType.MySql => new MySqlConnection(connectionString),
            DatabaseType.Sqlite => new SqliteConnection(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };
    }

    public static DbConnection CreateConnection(DatabaseType databaseType, string connectionString)
        => GetConnection(databaseType, connectionString);
}