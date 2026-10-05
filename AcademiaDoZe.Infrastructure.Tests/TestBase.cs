using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

//Carlos Eduardo Pomerening Stefanes
public abstract class TestBase
{
    // Alterne para DatabaseType.SqlServer, DatabaseType.MySql ou DatabaseType.Sqlite
    protected const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;

    // Dados do aluno/dev
    protected string SeuNome => "Carlos";
    protected string SeuSobrenome => "Stefanes";
    protected string SenhaSgbd => SelectedDatabaseType.ToString();

    protected string ConnectionString => SelectedDatabaseType switch
    {
        // SQL Server ajustado para a sua instância local 'ARBOLENDA\CARLOS'
        DatabaseType.SqlServer => @"Server=ARBOLENDA\CARLOS;Database=db_academia_do_ze;Integrated Security=True;TrustServerCertificate=True;",

        // MySQL (ajuste a senha se necessário)
        DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=Car23Los07;",

        // SQLite apontando para o seu arquivo real da D:\
        DatabaseType.Sqlite => @"Data Source=D:\atv 5\academia_test.db;",

        _ => throw new ArgumentOutOfRangeException()
    };
}