using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application;

public static class DatabaseTypeEnumMappingExtensions
{
    public static AppDatabaseType ToAppDatabaseType(this string providerName)
    {
        return providerName?.ToLower() switch
        {
            "sqlserver" => AppDatabaseType.SqlServer,
            "mysql" => AppDatabaseType.MySql,
            "sqlite" => AppDatabaseType.Sqlite,
            _ => AppDatabaseType.Sqlite
        };
    }
}