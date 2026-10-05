using AcademiaDoZe.Domain;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository<Colaborador>, IColaboradorRepository
{
    public ColaboradorRepository(DatabaseType databaseType, string connectionString)
        : base(databaseType, connectionString) { }

    private static string BaseSelectQuery => @"
        SELECT 
            c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email, 
            c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
            l.id_logradouro, l.cep, l.nome, l.bairro, l.cidade, l.uf, l.pais
        FROM tb_colaborador c
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public async Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE c.id_colaborador = @Id";
            AddParam(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter colaborador por ID.", ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} ORDER BY c.nome";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter todos os colaboradores.", ex);
        }
    }

    public async Task<Colaborador?> ObterPorCpf(string cpf, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE c.cpf = @Cpf";
            AddParam(command, "@Cpf", cpf);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter colaborador por CPF.", ex);
        }
    }

    public async Task<Colaborador?> ObterPorEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE c.email = @Email";
            AddParam(command, "@Email", email);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter colaborador por Email.", ex);
        }
    }

    public async Task<bool> CpfJaExiste(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM tb_colaborador WHERE cpf = @Cpf AND (@Id IS NULL OR id_colaborador <> @Id)";
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
            command.CommandText = "SELECT COUNT(1) FROM tb_colaborador WHERE email = @Email AND (@Id IS NULL OR id_colaborador <> @Id)";
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

    public async Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE c.tipo = @Tipo ORDER BY c.nome";
            AddParam(command, "@Tipo", (int)tipo);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter colaboradores por tipo.", ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $"{BaseSelectQuery} WHERE c.vinculo = @Vinculo ORDER BY c.nome";
            AddParam(command, "@Vinculo", (int)vinculo);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("Erro ao obter colaboradores por vínculo.", ex);
        }
    }

    public async Task<bool> TrocarSenha(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE tb_colaborador SET senha = @Senha WHERE id_colaborador = @Id";
            AddParam(command, "@Id", id);
            AddParam(command, "@Senha", novaSenha);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao trocar senha do colaborador ID {id}.", ex);
        }
    }

    public async Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();

            string insertQuery = @"INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) 
                                   VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo);";

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
            throw new InfrastructureException("Erro ao adicionar colaborador.", ex);
        }
    }

    public async Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE tb_colaborador SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, 
                                     email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, 
                                     senha = @Senha, foto = @Foto, admissao = @Admissao, tipo = @Tipo, vinculo = @Vinculo 
                                     WHERE id_colaborador = @Id";

            AddParam(command, "@Id", entity.Id);
            AddParameters(command, entity);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException($"Nenhum colaborador encontrado com ID {entity.Id}.");

            return entity;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao atualizar colaborador ID {entity.Id}.", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = DbProvider.GetConnection(DatabaseType, ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM tb_colaborador WHERE id_colaborador = @Id";
            AddParam(command, "@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException($"Erro ao remover colaborador ID {id}.", ex);
        }
    }

    public static Colaborador Map(DbDataReader reader)
    {
        int id = Convert.ToInt32(reader["id_colaborador"]);
        string cpf = reader["cpf"].ToString()!;
        string nome = reader["nome"].ToString()!;

        // Trata a conversão de Data de Nascimento de forma resiliente a qualquer formato/SGBD
        DateOnly dataNascimento = ParseDateOnly(reader["nascimento"]);

        string telefone = reader["telefone"].ToString()!;
        string email = reader["email"].ToString()!;
        string numero = reader["numero"].ToString()!;
        string complemento = reader["complemento"] == DBNull.Value ? string.Empty : reader["complemento"].ToString()!;
        string senha = reader["senha"].ToString()!;

        // Trata a conversão de Data de Admissão de forma resiliente a qualquer formato/SGBD
        DateOnly dataAdmissao = ParseDateOnly(reader["admissao"]);

        var tipo = (ColaboradorTipo)Convert.ToInt32(reader["tipo"]);
        var vinculo = (ColaboradorVinculo)Convert.ToInt32(reader["vinculo"]);

        byte[]? fotoBytes = reader["foto"] == DBNull.Value ? null : (byte[])reader["foto"];
        var foto = fotoBytes != null ? Arquivo.Criar(fotoBytes).Value : null;

        var logradouro = LogradouroRepository.Map(reader);

        var result = Colaborador.Criar(
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
            foto: foto,
            dataAdmissao: dataAdmissao,
            tipo: tipo,
            vinculo: vinculo
        );

        return result.Value!;
    }

    private static DateOnly ParseDateOnly(object value)
    {
        // 1. Se o banco já devolver como DateTime, apenas converte
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);

        // 2. Tenta parsear formatos padrão conhecidos do SQL Server / SGBDs
        string strVal = value.ToString()!.Trim();

        if (DateTime.TryParse(strVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedInv))
            return DateOnly.FromDateTime(parsedInv);

        if (DateTime.TryParse(strVal, new CultureInfo("pt-BR"), DateTimeStyles.None, out var parsedPt))
            return DateOnly.FromDateTime(parsedPt);

        if (DateTime.TryParse(strVal, new CultureInfo("en-US"), DateTimeStyles.None, out var parsedEn))
            return DateOnly.FromDateTime(parsedEn);

        // 3. Fallback: Substitui abreviações em português por inglês caso o parser do sistema falhe
        strVal = strVal.Replace("Mai", "May")
                       .Replace("Abo", "Abr")
                       .Replace("Ago", "Aug")
                       .Replace("Set", "Sep")
                       .Replace("Out", "Oct")
                       .Replace("Dez", "Dec");

        if (DateTime.TryParse(strVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedReplaced))
            return DateOnly.FromDateTime(parsedReplaced);

        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }

    private static void AddParam(DbCommand command, string name, object? value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        command.Parameters.Add(p);
    }

    private static void AddParameters(DbCommand command, Colaborador entity)
    {
        AddParam(command, "@Cpf", entity.Cpf.Valor);
        AddParam(command, "@Nome", entity.Nome);
        AddParam(command, "@Nascimento", entity.DataNascimento.ToDateTime(TimeOnly.MinValue));
        AddParam(command, "@Telefone", entity.Telefone.Valor);
        AddParam(command, "@Email", entity.Email.Valor);

        AddParam(command, "@LogradouroId", entity.Endereco.LogradouroId);

        AddParam(command, "@Numero", entity.Endereco.Numero);
        AddParam(command, "@Complemento", entity.Endereco.Complemento);
        AddParam(command, "@Senha", entity.Senha.Valor);

        var fotoParam = command.CreateParameter();
        fotoParam.ParameterName = "@Foto";
        fotoParam.Value = (object?)entity.Foto?.Conteudo ?? DBNull.Value;
        fotoParam.DbType = DbType.Binary;
        command.Parameters.Add(fotoParam);

        AddParam(command, "@Admissao", entity.DataAdmissao.ToDateTime(TimeOnly.MinValue));
        AddParam(command, "@Tipo", (int)entity.Tipo);
        AddParam(command, "@Vinculo", (int)entity.Vinculo);
    }
}