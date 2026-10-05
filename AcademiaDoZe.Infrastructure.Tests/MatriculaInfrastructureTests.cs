using AcademiaDoZe.Domain;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    // =========================================================================
    // EXIGÊNCIA DO PROFESSOR:
    // Altere a string abaixo colocando o seu NOME COMPLETO.
    // =========================================================================
    private const string NOME_DO_ALUNO = "Carlos Eduardo Pomerening Stefanes";

    private readonly MatriculaRepository _repository;
    private readonly AlunoRepository _alunoRepository;
    private readonly LogradouroRepository _logradouroRepository;

    public MatriculaInfrastructureTests()
    {
        _repository = new MatriculaRepository(SelectedDatabaseType, ConnectionString);
        _alunoRepository = new AlunoRepository(SelectedDatabaseType, ConnectionString);
        _logradouroRepository = new LogradouroRepository(SelectedDatabaseType, ConnectionString);
    }

    private string NomeSgbd => SelectedDatabaseType.ToString();

    private async Task<Aluno> CriarAlunoBaseAsync()
    {
        using (var connection = AcademiaDoZe.Infrastructure.DbProvider.GetConnection(SelectedDatabaseType, ConnectionString))
        {
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

            CREATE TABLE IF NOT EXISTS tb_matricula (
                id_matricula INTEGER PRIMARY KEY AUTOINCREMENT,
                aluno_id INTEGER NOT NULL,
                plano INTEGER NOT NULL,
                data_inicio TEXT NOT NULL,
                data_fim TEXT NOT NULL,
                objetivo TEXT,
                restricao_medica INTEGER NOT NULL DEFAULT 0,
                obs_restricao TEXT,
                laudo_medico BLOB,
                FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno)
            );

            INSERT OR IGNORE INTO tb_logradouro (id_logradouro, cep, nome, bairro, cidade, uf, pais)
            VALUES (1, '88501000', 'Rua dos Testes', 'Centro', 'Lages', 'SC', 'Brasil');
        ";
            await command.ExecuteNonQueryAsync();
        }

        var logradouro = Logradouro.Criar(
            1, "88501000", "Rua dos Testes", "Centro", "Lages", "SC", "Brasil"
        ).Value!;

        var aluno = Aluno.Criar(
            0,
            $"Aluno Teste {Random.Shared.Next(1000, 9999)}",
            $"{Random.Shared.Next(100, 999)}.456.789-{Random.Shared.Next(10, 99)}",
            new DateOnly(1998, 5, 20),
            "49999999999",
            $"aluno{Random.Shared.Next(10000, 99999)}@teste.com",
            logradouro,
            "123",
            "Apto 1",
            "SenhaValida123!",
            null
        ).Value!;

        return await _alunoRepository.Adicionar(aluno);
    }

    private async Task<Matricula> CriarMatriculaBaseAsync(Aluno aluno, MatriculaPlano plano = MatriculaPlano.Mensal, DateOnly? dataInicio = null)
    {
        var dtInicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);

        var matricula = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: plano,
            dataInicio: dtInicio,
            objetivo: NOME_DO_ALUNO, // Exigência do DEV 07: Nome completo no objetivo
            restricoesMedicas: MatriculaRestricoes.Nenhuma,
            laudoMedico: null,
            observacoesRestricoes: NomeSgbd // Exigência do DEV 07: Sigla/nome do SGBD na obs_restricao
        ).Value!;

        return await _repository.Adicionar(matricula);
    }

    [Fact]
    public async Task Adicionar_DeveInserirMatriculaComSucesso()
    {
        var aluno = await CriarAlunoBaseAsync();
        var matricula = await CriarMatriculaBaseAsync(aluno);

        Assert.True(matricula.Id > 0);
        Assert.Equal(NOME_DO_ALUNO, matricula.Objetivo);
        Assert.Equal(NomeSgbd, matricula.ObservacoesRestricoes);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarMatriculaExistente()
    {
        var aluno = await CriarAlunoBaseAsync();
        var matriculaCriada = await CriarMatriculaBaseAsync(aluno);

        var matriculaObtida = await _repository.ObterPorId(matriculaCriada.Id);

        Assert.NotNull(matriculaObtida);
        Assert.Equal(matriculaCriada.Id, matriculaObtida.Id);
        Assert.Equal(NOME_DO_ALUNO, matriculaObtida.Objetivo);
        Assert.Equal(NomeSgbd, matriculaObtida.ObservacoesRestricoes);
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarListaDeMatriculas()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno);

        var lista = await _repository.ObterTodos();

        Assert.NotNull(lista);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task Atualizar_DeveModificarDadosDaMatricula()
    {
        var aluno = await CriarAlunoBaseAsync();
        var matricula = await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Mensal);

        var novaMatricula = Matricula.Criar(
            id: matricula.Id,
            aluno: aluno,
            plano: MatriculaPlano.Anual,
            dataInicio: matricula.DataInicio,
            objetivo: NOME_DO_ALUNO,
            restricoesMedicas: MatriculaRestricoes.Medica,
            laudoMedico: null,
            observacoesRestricoes: NomeSgbd
        ).Value!;

        var matriculaAtualizada = await _repository.Atualizar(novaMatricula);
        var doBanco = await _repository.ObterPorId(matricula.Id);

        Assert.NotNull(doBanco);
        Assert.Equal(MatriculaPlano.Anual, doBanco.Plano);
        Assert.Equal(MatriculaRestricoes.Medica, doBanco.RestricoesMedicas);
    }

    [Fact]
    public async Task Remover_DeveExcluirMatriculaComSucesso()
    {
        var aluno = await CriarAlunoBaseAsync();
        var matricula = await CriarMatriculaBaseAsync(aluno);

        bool removido = await _repository.Remover(matricula.Id);
        var doBanco = await _repository.ObterPorId(matricula.Id);

        Assert.True(removido);
        Assert.Null(doBanco);
    }

    [Fact]
    public async Task ObterPorAluno_DeveRetornarMatriculasDoAluno()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno);

        var matriculas = await _repository.ObterPorAluno(aluno.Id);

        Assert.NotEmpty(matriculas);
        Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact]
    public async Task ObterMatriculaAtivaPorAluno_DeveRetornarMatriculaVigente()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Anual, DateOnly.FromDateTime(DateTime.Today));

        var ativa = await _repository.ObterMatriculaAtivaPorAluno(aluno.Id);

        Assert.NotNull(ativa);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact]
    public async Task PossuiMatriculaAtiva_DeveRetornarTrue_QuandoExistirMatriculaAtiva()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Anual, DateOnly.FromDateTime(DateTime.Today));

        bool possui = await _repository.PossuiMatriculaAtiva(aluno.Id);

        Assert.True(possui);
    }

    [Fact]
    public async Task ObterAtivas_DeveRetornarMatriculasVigentes()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Anual, DateOnly.FromDateTime(DateTime.Today));

        var ativas = await _repository.ObterAtivas();

        Assert.NotEmpty(ativas);
    }

    [Fact]
    public async Task ObterVencendoEmDias_DeveRetornarMatriculasProximasDoVencimento()
    {
        var aluno = await CriarAlunoBaseAsync();
        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-20));
        await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Mensal, inicio);

        var vencendo = await _repository.ObterVencendoEmDias(15);

        Assert.NotNull(vencendo);
    }

    [Fact]
    public async Task ObterPorPlano_DeveFiltrarPorPlanoCorreto()
    {
        var aluno = await CriarAlunoBaseAsync();
        await CriarMatriculaBaseAsync(aluno, MatriculaPlano.Semestral);

        var semestrais = await _repository.ObterPorPlano(MatriculaPlano.Semestral);

        Assert.NotEmpty(semestrais);
        Assert.All(semestrais, m => Assert.Equal(MatriculaPlano.Semestral, m.Plano));
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoIdNaoExiste()
    {
        var aluno = await CriarAlunoBaseAsync();
        var matriculaInexistente = Matricula.Criar(
            id: 999999,
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            dataInicio: DateOnly.FromDateTime(DateTime.Today),
            objetivo: NOME_DO_ALUNO,
            restricoesMedicas: MatriculaRestricoes.Nenhuma,
            laudoMedico: null,
            observacoesRestricoes: NomeSgbd
        ).Value!;

        await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(matriculaInexistente));
    }

    [Fact]
    public async Task Remover_DeveRetornarFalse_QuandoIdNaoExiste()
    {
        bool resultado = await _repository.Remover(999999);
        Assert.False(resultado);
    }
}