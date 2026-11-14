using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository<AcessoColaborador>, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType) { }

    public AcessoColaboradorRepository(DatabaseType databaseType, string connectionString)
        : base(connectionString, databaseType) { }

    private static string BaseSelectQuery => @"
        SELECT 
            acc.id_acesso_colaborador, acc.colaborador_id, acc.data_hora_entrada, acc.data_hora_saida,
            c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email, 
            c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.uf, l.pais
        FROM tb_acesso_colaborador acc
        INNER JOIN tb_colaborador c ON acc.colaborador_id = c.id_colaborador
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.id_acesso_colaborador = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acesso de colaborador por ID.", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY acc.data_hora_entrada DESC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todos os acessos de colaboradores.", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterPorColaboradorId(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.colaborador_id = @ColaboradorId ORDER BY acc.data_hora_entrada DESC";
            AddParam(command, "@ColaboradorId", colaboradorId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acessos por colaborador.", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcessoAberto(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE acc.colaborador_id = @ColaboradorId AND acc.data_hora_saida IS NULL ORDER BY acc.data_hora_entrada DESC";
            AddParam(command, "@ColaboradorId", colaboradorId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter último acesso em aberto do colaborador.", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosDeHoje(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

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
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter acessos de hoje do colaborador.", ex);
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasHoje(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            string dateCondition = DatabaseType switch
            {
                DatabaseType.SqlServer => "CAST(data_hora_entrada AS DATE) = CAST(GETDATE() AS DATE)",
                DatabaseType.MySql => "DATE(data_hora_entrada) = CURRENT_DATE()",
                DatabaseType.Sqlite => "DATE(data_hora_entrada) = DATE('now', 'localtime') OR DATE(data_hora_entrada) = DATE('now')",
                _ => "DATE(data_hora_entrada) = DATE('now')"
            };

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT data_hora_entrada, data_hora_saida FROM tb_acesso_colaborador WHERE colaborador_id = @ColaboradorId AND ({dateCondition})";
            AddParam(command, "@ColaboradorId", colaboradorId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            TimeSpan total = TimeSpan.Zero;
            while (await reader.ReadAsync(cancellationToken))
            {
                DateTime entrada = Convert.ToDateTime(reader["data_hora_entrada"]);
                DateTime saida = reader["data_hora_saida"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["data_hora_saida"]);
                if (saida > entrada)
                {
                    total += (saida - entrada);
                }
            }
            return total;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao calcular horas trabalhadas hoje.", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            string query = DatabaseType switch
            {
                DatabaseType.SqlServer => @"
                    INSERT INTO tb_acesso_colaborador (colaborador_id, data_hora_entrada, data_hora_saida) 
                    OUTPUT INSERTED.id_acesso_colaborador 
                    VALUES (@ColaboradorId, @DataHoraEntrada, @DataHoraSaida);",
                DatabaseType.MySql => @"
                    INSERT INTO tb_acesso_colaborador (colaborador_id, data_hora_entrada, data_hora_saida) 
                    VALUES (@ColaboradorId, @DataHoraEntrada, @DataHoraSaida); 
                    SELECT LAST_INSERT_ID();",
                DatabaseType.Sqlite => @"
                    INSERT INTO tb_acesso_colaborador (colaborador_id, data_hora_entrada, data_hora_saida) 
                    VALUES (@ColaboradorId, @DataHoraEntrada, @DataHoraSaida); 
                    SELECT last_insert_rowid();",
                _ => throw new ArgumentOutOfRangeException()
            };

            command.CommandText = query;
            AddParam(command, "@ColaboradorId", entity.ColaboradorId);
            AddParam(command, "@DataHoraEntrada", entity.DataHoraEntrada.ToString("yyyy-MM-dd HH:mm:ss"));
            AddParam(command, "@DataHoraSaida", entity.DataHoraSaida.HasValue ? entity.DataHoraSaida.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            entity.Id = Convert.ToInt32(result);
            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao adicionar acesso de colaborador.", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE tb_acesso_colaborador 
                SET data_hora_saida = @DataHoraSaida 
                WHERE id_acesso_colaborador = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParam(command, "@DataHoraSaida", entity.DataHoraSaida.HasValue ? entity.DataHoraSaida.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("Nenhum registro de acesso de colaborador atualizado.");

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

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_acesso_colaborador WHERE id_acesso_colaborador = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao remover acesso ID {id}.", ex);
        }
    }

    public static AcessoColaborador Map(DbDataReader reader)
    {
        int id = Convert.ToInt32(reader["id_acesso_colaborador"]);
        int colaboradorId = Convert.ToInt32(reader["colaborador_id"]);
        DateTime dataHoraEntrada = Convert.ToDateTime(reader["data_hora_entrada"]);
        DateTime? dataHoraSaida = reader["data_hora_saida"] == DBNull.Value ? null : Convert.ToDateTime(reader["data_hora_saida"]);

        Colaborador? colaborador = null;
        try
        {
            string colabNome = reader["colaborador_nome"]?.ToString() ?? reader["nome"]?.ToString() ?? "Colaborador";
            string colabCpf = reader["cpf"]?.ToString() ?? "000.000.000-00";
            DateOnly colabNasc = DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"]));
            string colabTel = reader["telefone"]?.ToString() ?? "";
            string colabEmail = reader["email"]?.ToString() ?? "";
            string colabNumero = reader["numero"]?.ToString() ?? "";
            string? colabCompl = reader["complemento"] == DBNull.Value ? null : reader["complemento"]?.ToString();
            string colabSenha = reader["senha"]?.ToString() ?? "";
            byte[]? colabFotoBytes = reader["foto"] == DBNull.Value ? null : (byte[])reader["foto"];
            Arquivo? colabFoto = colabFotoBytes != null ? Arquivo.Criar(colabFotoBytes).Value : null;
            DateOnly colabAdmissao = DateOnly.FromDateTime(Convert.ToDateTime(reader["admissao"]));
            var tipo = (ColaboradorTipo)Convert.ToInt32(reader["tipo"]);
            var vinculo = (ColaboradorVinculo)Convert.ToInt32(reader["vinculo"]);

            int logId = Convert.ToInt32(reader["id_logradouro"]);
            string logCep = reader["cep"]?.ToString() ?? "";
            string logNome = reader["logradouro_nome"]?.ToString() ?? "";
            string logBairro = reader["bairro"]?.ToString() ?? "";
            string logCidade = reader["cidade"]?.ToString() ?? "";
            string logUf = reader["uf"]?.ToString() ?? "";
            string logPais = reader["pais"]?.ToString() ?? "Brasil";
            var logradouro = Logradouro.Criar(logId, logCep, logNome, logBairro, logCidade, logUf, logPais).Value!;

            colaborador = Colaborador.Criar(colaboradorId, colabNome, colabCpf, colabNasc, colabTel, colabEmail, logradouro, colabNumero, colabCompl, colabSenha, colabFoto, colabAdmissao, tipo, vinculo).Value;
        }
        catch
        {
            // fallback se colunas adicionais não estiverem presentes
        }

        return AcessoColaborador.Criar(id, colaboradorId, dataHoraEntrada, dataHoraSaida, colaborador).Value!;
    }
}
