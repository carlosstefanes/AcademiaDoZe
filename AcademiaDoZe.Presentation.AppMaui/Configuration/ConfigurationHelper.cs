using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System.IO;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var repoConfig = ObterConfiguracaoAtual();
        services.AddSingleton(repoConfig);
        services.AddApplicationServices();

        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(
            repoConfig, (r, m) =>
            {
                var novaConfig = ObterConfiguracaoAtual();
                r.ConnectionString = novaConfig.ConnectionString;
                r.DatabaseType = novaConfig.DatabaseType;
            });
    }

    public static RepositoryConfig ObterConfiguracaoAtual()
    {
        string tipoStr = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        if (!Enum.TryParse<AppDatabaseType>(tipoStr, out var dbType))
            dbType = AppDatabaseType.Sqlite;

        string prefix = dbType.ToString();
        string connectionString;

        if (dbType == AppDatabaseType.Sqlite)
        {
            string dbPath = Preferences.Get($"{prefix}_Caminho", Preferences.Get("Database_Caminho", string.Empty));
            if (string.IsNullOrWhiteSpace(dbPath))
            {
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
            }

            string complemento = Preferences.Get($"{prefix}_Complemento", Preferences.Get("Database_Complemento", "Default Timeout=5;"));
            if (string.IsNullOrWhiteSpace(complemento))
                complemento = "Default Timeout=5;";

            if (!complemento.EndsWith(";"))
                complemento += ";";

            connectionString = $"Data Source={dbPath};{complemento}";
        }
        else
        {
            string dbServer = Preferences.Get($"{prefix}_Servidor", Preferences.Get("Database_Servidor", "localhost"));
            string dbDatabase = Preferences.Get($"{prefix}_Banco", Preferences.Get("Database_Banco", "AcademiaDoZe"));
            string dbUser = Preferences.Get($"{prefix}_Usuario", Preferences.Get("Database_Usuario", "root"));
            string dbPassword = Preferences.Get($"{prefix}_Senha", Preferences.Get("Database_Senha", string.Empty));
            string dbComplemento = Preferences.Get($"{prefix}_Complemento", Preferences.Get("Database_Complemento", string.Empty));

            if (string.IsNullOrWhiteSpace(dbComplemento))
            {
                if (dbType == AppDatabaseType.SqlServer)
                    dbComplemento = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
                else if (dbType == AppDatabaseType.MySql)
                    dbComplemento = "Connection Timeout=5;Default Command Timeout=30;";
            }

            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        return new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = dbType
        };
    }
}
