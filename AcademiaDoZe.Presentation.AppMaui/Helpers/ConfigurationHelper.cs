using AcademiaDoZe.Application;
using AcademiaDoZe.Application.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configuração padrão do provedor e string de conexão (ajuste conforme seu banco)
        var databaseType = AppDatabaseType.Sqlite;
        var connectionString = "Data Source=academiadoze.db";

        // Registrar a configuração de repositório se aplicável e registrar os serviços da aplicação
        // Exemplo: services.AddApplicationServices(databaseType, connectionString);
    }
}