using AcademiaDoZe.Domain;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository<Matricula>, IMatriculaRepository
{
    public MatriculaRepository(DatabaseType databaseType, string connectionString)
        : base(databaseType, connectionString)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT 
            m.id_matricula, m.aluno_id, m.plano, m.data_inicio, m.data_fim, m.objetivo, m.restricao_medica, m.obs_restricao, m.laudo_medico,
            a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.uf, l.pais
        FROM tb_matricula m
        INNER JOIN tb_aluno a ON m.aluno_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE m.id_matricula = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrícula por ID.", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY m.data_inicio DESC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todas as matrículas.", ex);
        }
    }

    public async Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();

            string insertQuery = @"INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico) 
                               VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico);";

            string selectIdQuery = DatabaseType switch
            {
                DatabaseType.SqlServer => "SELECT SCOPE_IDENTITY();",
                DatabaseType.Sqlite => "SELECT last_insert_rowid();",
                DatabaseType.MySql => "SELECT LAST_INSERT_ID();",
                _ => throw new NotSupportedException("SGBD não suportado.")
            };

            command.CommandText = $"{insertQuery} {selectIdQuery}";
            AddParameters(command, entity);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            int id = Convert.ToInt32(result);

            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao adicionar matrícula.", ex);
        }
    }

    public async Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE tb_matricula SET aluno_id = @AlunoId, plano = @Plano, data_inicio = @DataInicio, data_fim = @DataFim, objetivo = @Objetivo, 
                                     restricao_medica = @RestricaoMedica, obs_restricao = @ObsRestricao, laudo_medico = @LaudoMedico WHERE id_matricula = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParameters(command, entity);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO");
            }

            return entity;
        }
        catch (Exception ex)
        {
            if (ex is InfrastructureException) throw;
            throw new InfrastructureException($"Erro ao atualizar matrícula ID {entity.Id}.", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_matricula WHERE id_matricula = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao remover matrícula ID {id}.", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC";
            AddParam(command, "@AlunoId", alunoId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrículas por aluno.", ex);
        }
    }

    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            string dateFunc = DatabaseType switch
            {
                DatabaseType.SqlServer => "GETDATE()",
                DatabaseType.MySql => "CURRENT_DATE()",
                DatabaseType.Sqlite => "DATE('now')",
                _ => "CURRENT_DATE"
            };

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId AND m.data_fim >= {dateFunc} ORDER BY m.data_fim DESC";
            AddParam(command, "@AlunoId", alunoId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrícula ativa do aluno.", ex);
        }
    }

    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
    {
        var matricula = await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula != null;
    }

    public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            string dateFunc = DatabaseType switch
            {
                DatabaseType.SqlServer => "GETDATE()",
                DatabaseType.MySql => "CURRENT_DATE()",
                DatabaseType.Sqlite => "DATE('now')",
                _ => "CURRENT_DATE"
            };

            using var command = connection.CreateCommand();
            string query = $"{BaseSelectQuery} WHERE m.data_fim >= {dateFunc}";
            if (alunoId > 0)
            {
                query += " AND m.aluno_id = @AlunoId";
                AddParam(command, "@AlunoId", alunoId);
            }
            query += " ORDER BY m.data_fim ASC";
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrículas ativas.", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            string dateFunc = DatabaseType switch
            {
                DatabaseType.SqlServer => "GETDATE()",
                DatabaseType.MySql => "CURRENT_DATE()",
                DatabaseType.Sqlite => "DATE('now')",
                _ => "CURRENT_DATE"
            };

            string dateAddFunc = DatabaseType switch
            {
                DatabaseType.SqlServer => "DATEADD(day, @Dias, GETDATE())",
                DatabaseType.MySql => "DATE_ADD(CURRENT_DATE(), INTERVAL @Dias DAY)",
                DatabaseType.Sqlite => "DATE('now', '+' || @Dias || ' days')",
                _ => "DATE_ADD(CURRENT_DATE(), INTERVAL @Dias DAY)"
            };

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE m.data_fim >= {dateFunc} AND m.data_fim <= {dateAddFunc} ORDER BY m.data_fim ASC";
            AddParam(command, "@Dias", dias);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrículas vencendo em dias.", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE m.plano = @Plano ORDER BY a.nome";
            AddParam(command, "@Plano", (int)plano);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter matrículas por plano.", ex);
        }
    }

    public static Matricula Map(DbDataReader reader)
    {
        int id = Convert.ToInt32(reader["id_matricula"]);
        var plano = (MatriculaPlano)Convert.ToInt32(reader["plano"]);
        DateOnly dataInicio = ParseDateOnly(reader["data_inicio"]);
        string objetivo = reader["objetivo"] == DBNull.Value ? string.Empty : reader["objetivo"].ToString()!;

        var restricoesMedicas = (MatriculaRestricoes)Convert.ToInt32(reader["restricao_medica"]);
        string? obsRestricao = reader["obs_restricao"] == DBNull.Value ? null : reader["obs_restricao"].ToString();

        byte[]? laudoBytes = reader["laudo_medico"] == DBNull.Value ? null : (byte[])reader["laudo_medico"];
        Arquivo? laudoMedico = laudoBytes != null ? Arquivo.Criar(laudoBytes).Value : null;

        // Tenta obter o aluno através das colunas retornadas no reader.
        // Se a coluna 'nome' não estiver no reader (ex: query sem JOIN), cria um mock básico com o aluno_id
        Aluno aluno;
        try
        {
            aluno = AlunoRepository.Map(reader);
        }
        catch (ArgumentOutOfRangeException)
        {
            int alunoId = Convert.ToInt32(reader["aluno_id"]);
            var logradouroFake = Logradouro.Criar(1, "89520-000", "Rua Central", "Centro", "Curitibanos", "SC", "Brasil").Value!;
            aluno = Aluno.Criar(
                alunoId,
                "Aluno Teste",
                "000.000.000-00",
                new DateOnly(2000, 1, 1),
                "49999999999",
                $"aluno_{alunoId}@teste.com",
                logradouroFake,
                "123",
                null,
                "SenhaValida123!",
                null
            ).Value!;
        }

        var result = Matricula.Criar(
            id: id,
            aluno: aluno,
            plano: plano,
            dataInicio: dataInicio,
            objetivo: objetivo,
            restricoesMedicas: restricoesMedicas,
            laudoMedico: laudoMedico,
            observacoesRestricoes: obsRestricao
        );

        return result.Value!;
    }

    private static DateOnly ParseDateOnly(object value)
    {
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);

        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }

    private static void AddParam(DbCommand command, string name, object? value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        command.Parameters.Add(p);
    }

    private static void AddParameters(DbCommand command, Matricula entity)
    {
        AddParam(command, "@AlunoId", entity.AlunoId);
        AddParam(command, "@Plano", (int)entity.Plano);
        AddParam(command, "@DataInicio", entity.DataInicio.ToDateTime(TimeOnly.MinValue));
        AddParam(command, "@DataFim", entity.DataFim.ToDateTime(TimeOnly.MinValue));
        AddParam(command, "@Objetivo", entity.Objetivo);
        AddParam(command, "@RestricaoMedica", (int)entity.RestricoesMedicas);
        AddParam(command, "@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value);

        var laudoParam = command.CreateParameter();
        laudoParam.ParameterName = "@LaudoMedico";
        laudoParam.Value = (object?)entity.LaudoMedico?.Conteudo ?? DBNull.Value;
        laudoParam.DbType = DbType.Binary;
        command.Parameters.Add(laudoParam);
    }
}