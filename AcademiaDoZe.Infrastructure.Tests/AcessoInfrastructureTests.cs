using AcademiaDoZe.Domain;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoInfrastructureTests : TestBase
{
    private const string NOME_DO_DEV = "Carlos Eduardo Pomerening";

    private readonly AcessoAlunoRepository _acessoAlunoRepository;
    private readonly AcessoColaboradorRepository _acessoColaboradorRepository;
    private readonly AlunoRepository _alunoRepository;
    private readonly ColaboradorRepository _colaboradorRepository;
    private readonly LogradouroRepository _logradouroRepository;

    public AcessoInfrastructureTests()
    {
        _acessoAlunoRepository = new AcessoAlunoRepository(ConnectionString, SelectedDatabaseType);
        _acessoColaboradorRepository = new AcessoColaboradorRepository(ConnectionString, SelectedDatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, SelectedDatabaseType);
        _colaboradorRepository = new ColaboradorRepository(ConnectionString, SelectedDatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, SelectedDatabaseType);
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

            CREATE TABLE IF NOT EXISTS tb_aluno (
                id_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
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
                FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
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

            CREATE TABLE IF NOT EXISTS tb_acesso_aluno (
                id_acesso_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
                aluno_id INTEGER NOT NULL,
                data_hora_entrada TEXT NOT NULL,
                data_hora_saida TEXT,
                FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno)
            );

            CREATE TABLE IF NOT EXISTS tb_acesso_colaborador (
                id_acesso_colaborador INTEGER PRIMARY KEY AUTOINCREMENT,
                colaborador_id INTEGER NOT NULL,
                data_hora_entrada TEXT NOT NULL,
                data_hora_saida TEXT,
                FOREIGN KEY (colaborador_id) REFERENCES tb_colaborador(id_colaborador)
            );
        ";
        await command.ExecuteNonQueryAsync();
    }

    private async Task<Aluno> ObterOuCriarAlunoAsync()
    {
        await InicializarTabelasAsync();
        var alunos = (await _alunoRepository.ObterTodos()).ToList();
        if (alunos.Count > 0) return alunos[0];

        var logradouro = (await _logradouroRepository.ObterTodos()).FirstOrDefault();
        if (logradouro == null)
        {
            logradouro = Logradouro.Criar(0, "88501-000", "Rua Teste", "Centro", "Lages", "SC", "Brasil").Value!;
            logradouro = await _logradouroRepository.Adicionar(logradouro);
        }

        string sufixo = Guid.NewGuid().ToString("N")[..8];
        var novo = Aluno.Criar(0, NOME_DO_DEV, $"{Random.Shared.Next(100, 999)}.456.789-00", new DateOnly(2001, 7, 23), "49999999999", $"aluno_{sufixo}@teste.com", logradouro, "100", "Apto", "SenhaValida123!", null).Value!;
        return await _alunoRepository.Adicionar(novo);
    }

    private async Task<Colaborador> ObterOuCriarColaboradorAsync()
    {
        await InicializarTabelasAsync();
        var colabs = (await _colaboradorRepository.ObterTodos()).ToList();
        if (colabs.Count > 0) return colabs[0];

        var logradouro = (await _logradouroRepository.ObterTodos()).FirstOrDefault();
        if (logradouro == null)
        {
            logradouro = Logradouro.Criar(0, "88501-000", "Rua Teste", "Centro", "Lages", "SC", "Brasil").Value!;
            logradouro = await _logradouroRepository.Adicionar(logradouro);
        }

        string sufixo = Guid.NewGuid().ToString("N")[..8];
        var novo = Colaborador.Criar(0, NOME_DO_DEV, $"{Random.Shared.Next(100, 999)}.456.789-11", new DateOnly(2001, 7, 23), "49999999999", $"colab_{sufixo}@teste.com", logradouro, "100", "Apto", "SenhaValida123!", null, new DateOnly(2024, 1, 1), ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT).Value!;
        return await _colaboradorRepository.Adicionar(novo);
    }

    [Fact]
    public async Task Deve_Registrar_Entrada_E_Saida_Aluno()
    {
        var aluno = await ObterOuCriarAlunoAsync();

        // 1. Entrada
        var entrada = DateTime.Now;
        var acesso = AcessoAluno.Criar(0, aluno.Id, entrada).Value!;
        var adicionado = await _acessoAlunoRepository.Adicionar(acesso);

        Assert.True(adicionado.Id > 0);
        Assert.True(adicionado.EstaNaAcademia);

        // 2. Verifica se está aberto
        var aberto = await _acessoAlunoRepository.ObterUltimoAcessoAberto(aluno.Id);
        Assert.NotNull(aberto);
        Assert.Equal(adicionado.Id, aberto.Id);

        // 3. Saída
        var saida = aberto.DataHoraEntrada.AddMinutes(45);
        aberto.RegistrarSaida(saida);
        var atualizado = await _acessoAlunoRepository.Atualizar(aberto);

        Assert.NotNull(atualizado.DataHoraSaida);
        Assert.False(atualizado.EstaNaAcademia);
        Assert.Equal(TimeSpan.FromMinutes(45), atualizado.TempoPermanencia);
    }

    [Fact]
    public async Task Deve_Registrar_Entrada_E_Saida_Colaborador()
    {
        var colab = await ObterOuCriarColaboradorAsync();

        // 1. Entrada
        var entrada = DateTime.Now;
        var acesso = AcessoColaborador.Criar(0, colab.Id, entrada).Value!;
        var adicionado = await _acessoColaboradorRepository.Adicionar(acesso);

        Assert.True(adicionado.Id > 0);
        Assert.True(adicionado.EstaNaAcademia);

        // 2. Horas trabalhadas
        var horas = await _acessoColaboradorRepository.ObterHorasTrabalhadasHoje(colab.Id);
        Assert.True(horas >= TimeSpan.Zero);

        // 3. Saída
        var saida = entrada.AddHours(2);
        adicionado.RegistrarSaida(saida);
        var atualizado = await _acessoColaboradorRepository.Atualizar(adicionado);

        Assert.NotNull(atualizado.DataHoraSaida);
        Assert.False(atualizado.EstaNaAcademia);
    }
}
