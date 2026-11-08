using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Domain;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository<Logradouro>, ILogradouroRepository
{
    public LogradouroRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType) { }

    public LogradouroRepository(DatabaseType databaseType, string connectionString)
        : base(connectionString, databaseType) { }

    private static string BaseSelectQuery => "SELECT id_logradouro, cep, nome, bairro, cidade, uf, pais FROM tb_logradouro";

    public static Logradouro Map(DbDataReader reader)
    {
        int id = HasColumn(reader, "id_logradouro") && reader["id_logradouro"] != DBNull.Value
            ? Convert.ToInt32(reader["id_logradouro"])
            : (HasColumn(reader, "logradouro_id") && reader["logradouro_id"] != DBNull.Value ? Convert.ToInt32(reader["logradouro_id"]) : 0);

        string cep = HasColumn(reader, "cep") && reader["cep"] != DBNull.Value ? reader["cep"].ToString()! : string.Empty;

        string nome = HasColumn(reader, "logradouro_nome") && reader["logradouro_nome"] != DBNull.Value
            ? reader["logradouro_nome"].ToString()!
            : (HasColumn(reader, "nome") && reader["nome"] != DBNull.Value ? reader["nome"].ToString()! : string.Empty);

        string bairro = HasColumn(reader, "bairro") && reader["bairro"] != DBNull.Value ? reader["bairro"].ToString()! : string.Empty;
        string cidade = HasColumn(reader, "cidade") && reader["cidade"] != DBNull.Value ? reader["cidade"].ToString()! : string.Empty;

        string uf = HasColumn(reader, "uf") && reader["uf"] != DBNull.Value
            ? reader["uf"].ToString()!
            : (HasColumn(reader, "estado") && reader["estado"] != DBNull.Value ? reader["estado"].ToString()! : string.Empty);

        string pais = HasColumn(reader, "pais") && reader["pais"] != DBNull.Value ? reader["pais"].ToString()! : string.Empty;

        return Logradouro.Criar(id, cep, nome, bairro, cidade, uf, pais).Value!;
    }

    private static bool HasColumn(DbDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public async Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE id_logradouro = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter logradouro por ID.", ex);
        }
    }

    public Task<Logradouro?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => ObterPorId(id, cancellationToken);

    public async Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY nome";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<Logradouro>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todos os logradouros.", ex);
        }
    }

    public Task<IEnumerable<Logradouro>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => ObterTodos(cancellationToken);

    public async Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            string insertQuery = @"INSERT INTO tb_logradouro (cep, nome, bairro, cidade, uf, pais) 
                                   VALUES (@Cep, @Nome, @Bairro, @Cidade, @Uf, @Pais);";

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
            entity.Id = Convert.ToInt32(result);

            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao adicionar logradouro.", ex);
        }
    }

    public Task<Logradouro> InserirAsync(Logradouro entity, CancellationToken cancellationToken = default)
        => Adicionar(entity, cancellationToken);

    public async Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE tb_logradouro 
                                    SET cep = @Cep, nome = @Nome, bairro = @Bairro, cidade = @Cidade, uf = @Uf, pais = @Pais 
                                    WHERE id_logradouro = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParameters(command, entity);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao atualizar logradouro.", ex);
        }
    }

    public Task<Logradouro> AtualizarAsync(Logradouro entity, CancellationToken cancellationToken = default)
        => Atualizar(entity, cancellationToken);

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_logradouro WHERE id_logradouro = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao remover logradouro.", ex);
        }
    }

    public Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
        => Remover(id, cancellationToken);

    public async Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE cep = @Cep";
            AddParam(command, "@Cep", cep.Valor);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter logradouro por CEP.", ex);
        }
    }

    public async Task<bool> CepJaExiste(Cep cep, int? idDesconsiderar = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM tb_logradouro WHERE cep = @Cep AND (@Id IS NULL OR id_logradouro <> @Id)";
            AddParam(command, "@Cep", cep.Valor);
            AddParam(command, "@Id", (object?)idDesconsiderar ?? DBNull.Value);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao verificar existência de CEP.", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE cidade = @Cidade ORDER BY nome";
            AddParam(command, "@Cidade", cidade);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<Logradouro>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter logradouros por Cidade.", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE cidade = @Cidade AND bairro = @Bairro ORDER BY nome";
            AddParam(command, "@Cidade", cidade);
            AddParam(command, "@Bairro", bairro);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<Logradouro>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter logradouros por Bairro.", ex);
        }
    }

    private static void AddParameters(DbCommand command, Logradouro entity)
    {
        AddParam(command, "@Cep", entity.Cep);
        AddParam(command, "@Nome", entity.Nome);
        AddParam(command, "@Bairro", entity.Bairro);
        AddParam(command, "@Cidade", entity.Cidade);
        AddParam(command, "@Uf", entity.Uf);
        AddParam(command, "@Pais", entity.Pais);
    }
}
