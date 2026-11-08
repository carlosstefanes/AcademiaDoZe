using System;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appType) => appType switch
    {
        AppDatabaseType.SqlServer => DatabaseType.SqlServer,
        AppDatabaseType.MySql => DatabaseType.MySql,
        AppDatabaseType.Sqlite => DatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(appType), appType, null)
    };

    public static AppDatabaseType ToApplication(this DatabaseType infraType) => infraType switch
    {
        DatabaseType.SqlServer => AppDatabaseType.SqlServer,
        DatabaseType.MySql => AppDatabaseType.MySql,
        DatabaseType.Sqlite => AppDatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(infraType), infraType, null)
    };
}
