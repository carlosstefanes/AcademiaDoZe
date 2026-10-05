using AcademiaDoZe.Domain;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(SelectedDatabaseType, ConnectionString);
        _alunoRepo = new AlunoRepository(SelectedDatabaseType, ConnectionString);
    }

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        var todos = await _alunoRepo.ObterTodos(CancellationToken.None);
        Assert.NotNull(todos);
    }

    [Fact]
    public async Task Aluno_Adicionar_Sucesso()
    {
        // 1. Obter um logradouro existente para vincular o aluno
        var logradouros = await _logradouroRepo.ObterTodosAsync(CancellationToken.None);
        var logradouro = logradouros.FirstOrDefault();
        Assert.NotNull(logradouro);

        // 2. Criar o objeto respeitando as regras do slide:
        // Nome = SeuNome ("Carlos")
        // Complemento = SeuSobrenome ("Stefanes")
        // Senha = Sigla do SGBD ("MySql", "SqlServer" ou "Sqlite")
        var cpfRandom = new Random().Next(100000000, 999999999).ToString("D11");
        var emailRandom = $"aluno_{Guid.NewGuid().ToString()[..5]}@email.com";

        var alunoResult = Aluno.Criar(
            id: 0,
            nome: "Carlos",
            cpf: cpfRandom,
            dataNascimento: new DateOnly(2000, 1, 1),
            telefone: "49999999999",
            email: emailRandom,
            endereco: logradouro,
            numero: "100",
            complemento: "Stefanes",
            senha: SenhaSgbd,
            foto: null
        );

        Assert.True(alunoResult.IsSuccess);

        // 3. Adicionar no banco de dados
        var alunoAdicionado = await _alunoRepo.Adicionar(alunoResult.Value!, CancellationToken.None);
        Assert.NotNull(alunoAdicionado);
        Assert.True(alunoAdicionado.Id > 0);
    }
}