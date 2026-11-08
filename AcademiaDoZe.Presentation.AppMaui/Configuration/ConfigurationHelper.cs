using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System.IO;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var databaseType = AppDatabaseType.Sqlite;
        string connectionString;
        if (databaseType == AppDatabaseType.Sqlite)
        {
            string dbPath;
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                if (File.Exists(@"D:\atv 5\academia_test.db"))
                    dbPath = @"D:\atv 5\academia_test.db";
                else if (File.Exists(@"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"))
                    dbPath = @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db";
                else
                    dbPath = @"D:\atv 5\academia_test.db";
            }
            else
            {
                dbPath = Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
            }
            connectionString = $"Data Source={dbPath};Default Timeout=5;";
        }
        else
        {
            const string dbServer = "10.30.21.16";
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "root";
            const string dbPassword = "abcBolinhas12345";
            string dbComplemento = string.Empty;
            if (databaseType == AppDatabaseType.SqlServer)
                dbComplemento = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
            else if (databaseType == AppDatabaseType.MySql)
                dbComplemento = "Connection Timeout=5;Default Command Timeout=30;";
            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType
        });
        services.AddApplicationServices();
    }
}
