using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DependencyInjection;

public class RepositoryConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public AppDatabaseType DatabaseType { get; set; } = AppDatabaseType.Sqlite;
}

