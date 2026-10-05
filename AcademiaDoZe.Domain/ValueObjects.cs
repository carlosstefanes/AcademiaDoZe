namespace AcademiaDoZe.Domain;

public record Cpf(string Valor)
{
    public static Result<Cpf> Criar(string valor) => Result<Cpf>.Success(new Cpf(valor));
}

public record Telefone(string Valor)
{
    public static Result<Telefone> Criar(string valor) => Result<Telefone>.Success(new Telefone(valor));
}

public record Email(string Valor)
{
    public static Result<Email> Criar(string valor) => Result<Email>.Success(new Email(valor));
}

public record Senha(string Valor)
{
    public static Result<Senha> Criar(string valor) => Result<Senha>.Success(new Senha(valor));
}

public record Endereco(int LogradouroId, string Numero, string? Complemento)
{
    public static Result<Endereco> Criar(int logradouroId, string numero, string? complemento)
        => Result<Endereco>.Success(new Endereco(logradouroId, numero, complemento));
}

public record Arquivo(byte[] Conteudo)
{
    public static Result<Arquivo> Criar(byte[] conteudo) => Result<Arquivo>.Success(new Arquivo(conteudo));
}