using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Domain;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository<Logradouro>, ILogradouroRepository
{
    public LogradouroRepository(DatabaseType databaseType, string connectionString)
        : base(databaseType, connectionString) { }

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

    public async Task<Logradouro?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id_logradouro, cep, nome, bairro, cidade, uf, pais FROM tb_logradouro WHERE id_logradouro = @Id";

        var p = command.CreateParameter();
        p.ParameterName = "@Id";
        p.Value = id;
        command.Parameters.Add(p);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IEnumerable<Logradouro>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id_logradouro, cep, nome, bairro, cidade, uf, pais FROM tb_logradouro";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var lista = new List<Logradouro>();
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(Map(reader));
        }
        return lista;
    }

    public async Task<Logradouro> InserirAsync(Logradouro entity, CancellationToken cancellationToken = default)
    {
        using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO tb_logradouro (cep, nome, bairro, cidade, uf, pais) 
                                 VALUES (@Cep, @Nome, @Bairro, @Cidade, @Uf, @Pais);
                                 SELECT LAST_INSERT_ID();";

        AddParameters(command, entity);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        entity.Id = Convert.ToInt32(result);

        return entity;
    }

    public async Task<Logradouro> AtualizarAsync(Logradouro entity, CancellationToken cancellationToken = default)
    {
        using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE tb_logradouro SET cep = @Cep, nome = @Nome, bairro = @Bairro, cidade = @Cidade, uf = @Uf, pais = @Pais WHERE id_logradouro = @Id";

        var pId = command.CreateParameter();
        pId.ParameterName = "@Id";
        pId.Value = entity.Id;
        command.Parameters.Add(pId);

        AddParameters(command, entity);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tb_logradouro WHERE id_logradouro = @Id";

        var p = command.CreateParameter();
        p.ParameterName = "@Id";
        p.Value = id;
        command.Parameters.Add(p);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static void AddParameters(DbCommand command, Logradouro entity)
    {
        void Add(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        Add("@Cep", entity.Cep);
        Add("@Nome", entity.Nome);
        Add("@Bairro", entity.Bairro);
        Add("@Cidade", entity.Cidade);
        Add("@Uf", entity.Uf);
        Add("@Pais", entity.Pais);
    }
}