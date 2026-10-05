using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace AcademiaDoZe.Infrastructure
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DatabaseType dbType, string connectionString)
        {
            try
            {
                switch (dbType)
                {
                    case DatabaseType.SqlServer:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            var sql = @"
                                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tb_logradouro')
                                BEGIN
                                    CREATE TABLE tb_logradouro (
                                        id_logradouro INT IDENTITY(1,1) PRIMARY KEY,
                                        cep VARCHAR(10) NOT NULL,
                                        nome VARCHAR(150) NOT NULL,
                                        bairro VARCHAR(100) NOT NULL,
                                        cidade VARCHAR(100) NOT NULL,
                                        uf VARCHAR(2) NOT NULL,
                                        pais VARCHAR(50) NOT NULL
                                    );
                                END

                                IF NOT EXISTS (SELECT 1 FROM tb_logradouro WHERE id_logradouro = 1)
                                BEGIN
                                    SET IDENTITY_INSERT tb_logradouro ON;
                                    INSERT INTO tb_logradouro (id_logradouro, cep, nome, bairro, cidade, uf, pais)
                                    VALUES (1, '89520-000', 'Rua Central', 'Centro', 'Curitibanos', 'SC', 'Brasil');
                                    SET IDENTITY_INSERT tb_logradouro OFF;
                                END

                                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tb_colaborador')
                                BEGIN
                                    CREATE TABLE tb_colaborador (
                                        id_colaborador INT IDENTITY(1,1) PRIMARY KEY,
                                        cpf VARCHAR(20) NOT NULL UNIQUE,
                                        nome VARCHAR(150) NOT NULL,
                                        nascimento VARCHAR(20) NOT NULL,
                                        telefone VARCHAR(20) NOT NULL,
                                        email VARCHAR(100) NOT NULL UNIQUE,
                                        logradouro_id INT NOT NULL,
                                        numero VARCHAR(20) NOT NULL,
                                        complemento VARCHAR(50) NULL,
                                        senha VARCHAR(50) NOT NULL,
                                        foto VARBINARY(MAX) NULL,
                                        admissao VARCHAR(20) NOT NULL,
                                        tipo INT NOT NULL,
                                        vinculo INT NOT NULL,
                                        CONSTRAINT FK_colaborador_logradouro FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
                                    );
                                END

                                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tb_aluno')
                                BEGIN
                                    CREATE TABLE tb_aluno (
                                        id_aluno INT IDENTITY(1,1) PRIMARY KEY,
                                        cpf VARCHAR(20) NOT NULL UNIQUE,
                                        nome VARCHAR(150) NOT NULL,
                                        nascimento VARCHAR(20) NOT NULL,
                                        telefone VARCHAR(20) NOT NULL,
                                        email VARCHAR(100) NOT NULL UNIQUE,
                                        logradouro_id INT NOT NULL,
                                        numero VARCHAR(20) NOT NULL,
                                        complemento VARCHAR(50) NULL,
                                        senha VARCHAR(50) NOT NULL,
                                        foto VARBINARY(MAX) NULL,
                                        CONSTRAINT FK_aluno_logradouro FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
                                    );
                                END

                                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tb_matricula')
                                BEGIN
                                    CREATE TABLE tb_matricula (
                                        id_matricula INT IDENTITY(1,1) PRIMARY KEY,
                                        aluno_id INT NOT NULL,
                                        plano INT NOT NULL,
                                        data_inicio DATETIME NOT NULL,
                                        data_fim DATETIME NOT NULL,
                                        objetivo VARCHAR(255) NULL,
                                        restricao_medica INT NOT NULL,
                                        obs_restricao VARCHAR(255) NULL,
                                        laudo_medico VARBINARY(MAX) NULL,
                                        CONSTRAINT FK_matricula_aluno FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno)
                                    );
                                END";

                            using (var command = new SqlCommand(sql, connection))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        break;

                    case DatabaseType.MySql:
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            var sql = @"
                                CREATE TABLE IF NOT EXISTS tb_logradouro (
                                    id_logradouro INT AUTO_INCREMENT PRIMARY KEY,
                                    cep VARCHAR(10) NOT NULL,
                                    nome VARCHAR(150) NOT NULL,
                                    bairro VARCHAR(100) NOT NULL,
                                    cidade VARCHAR(100) NOT NULL,
                                    uf VARCHAR(2) NOT NULL,
                                    pais VARCHAR(50) NOT NULL
                                );

                                INSERT IGNORE INTO tb_logradouro (id_logradouro, cep, nome, bairro, cidade, uf, pais)
                                VALUES (1, '89520-000', 'Rua Central', 'Centro', 'Curitibanos', 'SC', 'Brasil');

                                CREATE TABLE IF NOT EXISTS tb_colaborador (
                                    id_colaborador INT AUTO_INCREMENT PRIMARY KEY,
                                    cpf VARCHAR(20) NOT NULL UNIQUE,
                                    nome VARCHAR(150) NOT NULL,
                                    nascimento VARCHAR(20) NOT NULL,
                                    telefone VARCHAR(20) NOT NULL,
                                    email VARCHAR(100) NOT NULL UNIQUE,
                                    logradouro_id INT NOT NULL,
                                    numero VARCHAR(20) NOT NULL,
                                    complemento VARCHAR(50) NULL,
                                    senha VARCHAR(50) NOT NULL,
                                    foto LONGBLOB NULL,
                                    admissao VARCHAR(20) NOT NULL,
                                    tipo INT NOT NULL,
                                    vinculo INT NOT NULL,
                                    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
                                );

                                CREATE TABLE IF NOT EXISTS tb_aluno (
                                    id_aluno INT AUTO_INCREMENT PRIMARY KEY,
                                    cpf VARCHAR(20) NOT NULL UNIQUE,
                                    nome VARCHAR(150) NOT NULL,
                                    nascimento VARCHAR(20) NOT NULL,
                                    telefone VARCHAR(20) NOT NULL,
                                    email VARCHAR(100) NOT NULL UNIQUE,
                                    logradouro_id INT NOT NULL,
                                    numero VARCHAR(20) NOT NULL,
                                    complemento VARCHAR(50) NULL,
                                    senha VARCHAR(50) NOT NULL,
                                    foto LONGBLOB NULL,
                                    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
                                );

                                CREATE TABLE IF NOT EXISTS tb_matricula (
                                    id_matricula INT AUTO_INCREMENT PRIMARY KEY,
                                    aluno_id INT NOT NULL,
                                    plano INT NOT NULL,
                                    data_inicio DATETIME NOT NULL,
                                    data_fim DATETIME NOT NULL,
                                    objetivo VARCHAR(255) NULL,
                                    restricao_medica INT NOT NULL,
                                    obs_restricao VARCHAR(255) NULL,
                                    laudo_medico LONGBLOB NULL,
                                    FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno)
                                );";

                            using (var command = new MySqlCommand(sql, connection))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(dbType), $"Tipo de banco não suportado: {dbType}");
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erro ao inicializar o banco de dados.", ex);
            }
        }
    }
}