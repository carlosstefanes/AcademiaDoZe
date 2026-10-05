using AcademiaDoZe.Domain;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AlunoRepository : BaseRepository<Aluno>, IAlunoRepository
{
    public AlunoRepository(DatabaseType databaseType, string connectionString)
        : base(databaseType, connectionString) { }

    private static string BaseSelectQuery => @"
        SELECT 
            a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, 
            a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
            l.id_logradouro, l.cep, l.nome, l.bairro, l.cidade, l.uf, l.pais
        FROM tb_aluno a
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public async Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE a.id_aluno = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter aluno por ID.", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY a.nome";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var alunos = new List<Aluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                alunos.Add(Map(reader));
            }
            return alunos;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todos os alunos.", ex);
        }
    }

    public async Task<Aluno?> ObterPorCpf(string cpf, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE a.cpf = @Cpf";
            AddParam(command, "@Cpf", cpf);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter aluno por CPF.", ex);
        }
    }

    public async Task<Aluno?> ObterPorEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE a.email = @Email";
            AddParam(command, "@Email", email);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter aluno por Email.", ex);
        }
    }

    public async Task<bool> CpfJaExiste(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM tb_aluno WHERE cpf = @Cpf AND (@Id IS NULL OR id_aluno <> @Id)";
            AddParam(command, "@Cpf", cpf);
            AddParam(command, "@Id", (object?)id ?? DBNull.Value);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao verificar existência de CPF.", ex);
        }
    }

    public async Task<bool> EmailJaExiste(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM tb_aluno WHERE email = @Email AND (@Id IS NULL OR id_aluno <> @Id)";
            AddParam(command, "@Email", email);
            AddParam(command, "@Id", (object?)id ?? DBNull.Value);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao verificar existência de Email.", ex);
        }
    }

    public async Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();

            string insertQuery = @"INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto) 
                                   VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto);";

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
            throw new InfrastructureException("Erro ao adicionar aluno.", ex);
        }
    }

    public async Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE tb_aluno SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, 
                                     email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, 
                                     senha = @Senha, foto = @Foto WHERE id_aluno = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParameters(command, entity);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException($"Nenhum aluno encontrado com ID {entity.Id}.");

            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao atualizar aluno ID {entity.Id}.", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_aluno WHERE id_aluno = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao remover aluno ID {id}.", ex);
        }
    }

    public static Aluno Map(DbDataReader reader)
    {
        int id = Convert.ToInt32(reader["id_aluno"]);
        string cpf = reader["cpf"].ToString()!;
        string nome = reader["nome"].ToString()!;
        DateOnly dataNascimento = DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"]));
        string telefone = reader["telefone"].ToString()!;
        string email = reader["email"].ToString()!;
        string numero = reader["numero"].ToString()!;
        string complemento = reader["complemento"] == DBNull.Value ? string.Empty : reader["complemento"].ToString()!;
        string senha = reader["senha"].ToString()!;

        byte[]? fotoBytes = reader["foto"] == DBNull.Value ? null : (byte[])reader["foto"];
        var foto = fotoBytes != null ? Arquivo.Criar(fotoBytes).Value : null;

        var logradouro = LogradouroRepository.Map(reader);

        var result = Aluno.Criar(
            id: id,
            nome: nome,
            cpf: cpf,
            dataNascimento: dataNascimento,
            telefone: telefone,
            email: email,
            endereco: logradouro,
            numero: numero,
            complemento: complemento,
            senha: senha,
            foto: foto
        );

        return result.Value!;
    }

    private static void AddParam(DbCommand command, string name, object? value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        command.Parameters.Add(p);
    }

    private static void AddParameters(DbCommand command, Aluno entity)
    {
        AddParam(command, "@Cpf", entity.Cpf.Valor);
        AddParam(command, "@Nome", entity.Nome);
        AddParam(command, "@Nascimento", entity.DataNascimento.ToDateTime(TimeOnly.MinValue));
        AddParam(command, "@Telefone", entity.Telefone.Valor);
        AddParam(command, "@Email", entity.Email.Valor);

        // CORREÇÃO AQUI: Passar apenas o LogradouroId (ou entity.Endereco.Id)
        AddParam(command, "@LogradouroId", entity.Endereco.LogradouroId);

        AddParam(command, "@Numero", entity.Endereco.Numero);
        AddParam(command, "@Complemento", entity.Endereco.Complemento);
        AddParam(command, "@Senha", entity.Senha.Valor);
        // EM VEZ DE:
        // AddParam(command, "@Foto", entity.Foto?.Conteudo);

        // COLOQUE ASSIM:
        var fotoParam = command.CreateParameter();
        fotoParam.ParameterName = "@Foto";
        fotoParam.Value = (object?)entity.Foto?.Conteudo ?? DBNull.Value;
        fotoParam.DbType = System.Data.DbType.Binary; // Força o tipo varbinary/binary explicitamente
        command.Parameters.Add(fotoParam);
    }
}