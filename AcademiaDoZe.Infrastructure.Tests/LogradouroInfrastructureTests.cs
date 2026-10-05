using AcademiaDoZe.Domain;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;

    public LogradouroInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(SelectedDatabaseType, ConnectionString);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        // Alterado de ObterTodos() para ObterTodosAsync()
        var todos = await _logradouroRepo.ObterTodosAsync();
        Assert.NotNull(todos);
    }
}