using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository<AcessoAluno>, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType) { }

    public AcessoAlunoRepository(DatabaseType databaseType, string connectionString)
        : base(connectionString, databaseType) { }

    private static bool _tableEnsured = false;
    private static readonly object _lock = new();

    private void EnsureTableCreated(DbConnection connection)
    {
        if (_tableEnsured) return;
        lock (_lock)
        {
            if (_tableEnsured) return;
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = DatabaseType switch
                {
                    DatabaseType.SqlServer => @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tb_acesso_aluno' AND xtype='U')
                        CREATE TABLE tb_acesso_aluno (
                            id_acesso_aluno INT IDENTITY(1,1) PRIMARY KEY,
                            aluno_id INT NOT NULL,
                            data_hora_entrada DATETIME NOT NULL,
                            data_hora_saida DATETIME NULL
                        );",
                    DatabaseType.MySql => @"
                        CREATE TABLE IF NOT EXISTS tb_acesso_aluno (
                            id_acesso_aluno INT AUTO_INCREMENT PRIMARY KEY,
                            aluno_id INT NOT NULL,
                            data_hora_entrada DATETIME NOT NULL,
                            data_hora_saida DATETIME NULL
                        );",
                    _ => @"
                        CREATE TABLE IF NOT EXISTS tb_acesso_aluno (
                            id_acesso_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
                            aluno_id INTEGER NOT NULL,
                            data_hora_entrada TEXT NOT NULL,
                            data_hora_saida TEXT
                        );"
                };
                cmd.ExecuteNonQuery();
                _tableEnsured = true;
            }
            catch { }
        }
    }

    private static string BaseSelectQuery => @"
        SELECT 
            acc.id_acesso_aluno, acc.aluno_id, acc.data_hora_entrada, acc.data_hora_saida,
            a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.uf, l.pais
        FROM tb_acesso_aluno acc
        INNER JOIN tb_aluno a ON acc.aluno_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.id_acesso_aluno = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acesso do aluno por ID.", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY acc.data_hora_entrada DESC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todos os acessos de alunos.", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterPorAlunoId(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.aluno_id = @AlunoId ORDER BY acc.data_hora_entrada DESC";
            AddParam(command, "@AlunoId", alunoId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acessos por aluno.", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcessoAberto(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.aluno_id = @AlunoId AND acc.data_hora_saida IS NULL ORDER BY acc.data_hora_entrada DESC";
            AddParam(command, "@AlunoId", alunoId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter último acesso em aberto do aluno.", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosDeHoje(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            string dateCondition = DatabaseType switch
            {
                DatabaseType.SqlServer => "CAST(acc.data_hora_entrada AS DATE) = CAST(GETDATE() AS DATE)",
                DatabaseType.MySql => "DATE(acc.data_hora_entrada) = CURRENT_DATE()",
                DatabaseType.Sqlite => "DATE(acc.data_hora_entrada) = DATE('now', 'localtime') OR DATE(acc.data_hora_entrada) = DATE('now')",
                _ => "DATE(acc.data_hora_entrada) = DATE('now')"
            };

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE {dateCondition} ORDER BY acc.data_hora_entrada DESC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acessos de hoje do aluno.", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            string query = DatabaseType switch
            {
                DatabaseType.SqlServer => @"
                    INSERT INTO tb_acesso_aluno (aluno_id, data_hora_entrada, data_hora_saida) 
                    OUTPUT INSERTED.id_acesso_aluno 
                    VALUES (@AlunoId, @DataHoraEntrada, @DataHoraSaida);",
                DatabaseType.MySql => @"
                    INSERT INTO tb_acesso_aluno (aluno_id, data_hora_entrada, data_hora_saida) 
                    VALUES (@AlunoId, @DataHoraEntrada, @DataHoraSaida); 
                    SELECT LAST_INSERT_ID();",
                DatabaseType.Sqlite => @"
                    INSERT INTO tb_acesso_aluno (aluno_id, data_hora_entrada, data_hora_saida) 
                    VALUES (@AlunoId, @DataHoraEntrada, @DataHoraSaida); 
                    SELECT last_insert_rowid();",
                _ => throw new ArgumentOutOfRangeException()
            };

            command.CommandText = query;
            AddParam(command, "@AlunoId", entity.AlunoId);
            AddParam(command, "@DataHoraEntrada", entity.DataHoraEntrada.ToString("yyyy-MM-dd HH:mm:ss"));
            AddParam(command, "@DataHoraSaida", entity.DataHoraSaida.HasValue ? entity.DataHoraSaida.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            entity.Id = Convert.ToInt32(result);
            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao adicionar acesso de aluno.", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE tb_acesso_aluno 
                SET data_hora_saida = @DataHoraSaida 
                WHERE id_acesso_aluno = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParam(command, "@DataHoraSaida", entity.DataHoraSaida.HasValue ? entity.DataHoraSaida.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("Nenhum registro de acesso de aluno atualizado.");

            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao atualizar acesso ID {entity.Id}.", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);
            EnsureTableCreated(connection);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_acesso_aluno WHERE id_acesso_aluno = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao remover acesso ID {id}.", ex);
        }
    }

    public static AcessoAluno Map(DbDataReader reader)
    {
        int id = Convert.ToInt32(reader["id_acesso_aluno"]);
        int alunoId = Convert.ToInt32(reader["aluno_id"]);
        DateTime dataHoraEntrada = Convert.ToDateTime(reader["data_hora_entrada"]);
        DateTime? dataHoraSaida = reader["data_hora_saida"] == DBNull.Value ? null : Convert.ToDateTime(reader["data_hora_saida"]);

        Aluno? aluno = null;
        try
        {
            string alunoNome = reader["aluno_nome"]?.ToString() ?? reader["nome"]?.ToString() ?? "Aluno";
            string alunoCpf = reader["cpf"]?.ToString() ?? "000.000.000-00";
            DateOnly alunoNasc = DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"]));
            string alunoTel = reader["telefone"]?.ToString() ?? "";
            string alunoEmail = reader["email"]?.ToString() ?? "";
            string alunoNumero = reader["numero"]?.ToString() ?? "";
            string? alunoCompl = reader["complemento"] == DBNull.Value ? null : reader["complemento"]?.ToString();
            string alunoSenha = reader["senha"]?.ToString() ?? "";
            byte[]? alunoFotoBytes = reader["foto"] == DBNull.Value ? null : (byte[])reader["foto"];
            Arquivo? alunoFoto = alunoFotoBytes != null ? Arquivo.Criar(alunoFotoBytes).Value : null;

            int logId = Convert.ToInt32(reader["id_logradouro"]);
            string logCep = reader["cep"]?.ToString() ?? "";
            string logNome = reader["logradouro_nome"]?.ToString() ?? "";
            string logBairro = reader["bairro"]?.ToString() ?? "";
            string logCidade = reader["cidade"]?.ToString() ?? "";
            string logUf = reader["uf"]?.ToString() ?? "";
            string logPais = reader["pais"]?.ToString() ?? "Brasil";
            var logradouro = Logradouro.Criar(logId, logCep, logNome, logBairro, logCidade, logUf, logPais).Value!;

            aluno = Aluno.Criar(alunoId, alunoNome, alunoCpf, alunoNasc, alunoTel, alunoEmail, logradouro, alunoNumero, alunoCompl, alunoSenha, alunoFoto).Value;
        }
        catch
        {
            // fallback se colunas adicionais não estiverem presentes
        }

        return AcessoAluno.Criar(id, alunoId, dataHoraEntrada, dataHoraSaida, aluno).Value!;
    }
}
