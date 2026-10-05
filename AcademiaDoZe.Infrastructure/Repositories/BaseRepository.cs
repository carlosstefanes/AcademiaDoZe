using AcademiaDoZe.Domain;
using AcademiaDoZe.Infrastructure;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository<T> where T : Entity
{
    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected BaseRepository(DatabaseType databaseType, string connectionString)
    {
        DatabaseType = databaseType;
        ConnectionString = connectionString;
    }
    protected static void AddParam(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;

        // Se o valor for um byte array (foto), garante DbType.Binary
        if (value is byte[])
        {
            parameter.DbType = System.Data.DbType.Binary;
        }

        command.Parameters.Add(parameter);
    }
}
