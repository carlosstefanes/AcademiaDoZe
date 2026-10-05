using AcademiaDoZe.Domain;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(SelectedDatabaseType, ConnectionString);
        _colaboradorRepo = new ColaboradorRepository(SelectedDatabaseType, ConnectionString);

        // Garante que o banco e as tabelas do colaborador existam antes de executar qualquer teste
        InicializarTabelasAsync().GetAwaiter().GetResult();
    }

    private async Task InicializarTabelasAsync()
    {
        using var connection = DbProvider.GetConnection(SelectedDatabaseType, ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS tb_logradouro (
                id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
                cep TEXT NOT NULL,
                nome TEXT NOT NULL,
                bairro TEXT NOT NULL,
                cidade TEXT NOT NULL,
                uf TEXT NOT NULL,
                pais TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS tb_colaborador (
                id_colaborador INTEGER PRIMARY KEY AUTOINCREMENT,
                cpf TEXT NOT NULL UNIQUE,
                nome TEXT NOT NULL,
                nascimento TEXT NOT NULL,
                telefone TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                logradouro_id INTEGER NOT NULL,
                numero TEXT NOT NULL,
                complemento TEXT,
                senha TEXT NOT NULL,
                foto BLOB,
                admissao TEXT NOT NULL,
                tipo INTEGER NOT NULL,
                vinculo INTEGER NOT NULL,
                FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
            );

            INSERT OR IGNORE INTO tb_logradouro (id_logradouro, cep, nome, bairro, cidade, uf, pais)
            VALUES (1, '88501000', 'Rua dos Testes', 'Centro', 'Lages', 'SC', 'Brasil');
        ";
        await command.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        var todos = await _colaboradorRepo.ObterTodos();
        Assert.NotNull(todos);
    }

    [Fact]
    public async Task Colaborador_Adicionar_Sucesso()
    {
        // 1. Obter um logradouro existente
        var logradouros = await _logradouroRepo.ObterTodosAsync(CancellationToken.None);
        var logradouro = logradouros.FirstOrDefault();
        Assert.NotNull(logradouro);

        // 2. Criar o colaborador respeitando as regras do slide
        var cpfRandom = new Random().Next(100000000, 999999999).ToString("D11");
        var emailRandom = $"colab_{Guid.NewGuid().ToString()[..5]}@email.com";

        var colaboradorResult = Colaborador.Criar(
            id: 0,
            nome: "Carlos",
            cpf: cpfRandom,
            dataNascimento: new DateOnly(1995, 5, 10),
            telefone: "49888888888",
            email: emailRandom,
            endereco: logradouro,
            numero: "200",
            complemento: "Stefanes",
            senha: SenhaSgbd,
            foto: null,
            dataAdmissao: DateOnly.FromDateTime(DateTime.Now),
            tipo: ColaboradorTipo.Administrador,
            vinculo: ColaboradorVinculo.CLT
        );

        Assert.True(colaboradorResult.IsSuccess);

        // 3. Adicionar no banco de dados
        var colaboradorAdicionado = await _colaboradorRepo.Adicionar(colaboradorResult.Value!, CancellationToken.None);
        Assert.NotNull(colaboradorAdicionado);
        Assert.True(colaboradorAdicionado.Id > 0);
    }
}