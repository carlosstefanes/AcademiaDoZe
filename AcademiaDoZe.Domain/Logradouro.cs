namespace AcademiaDoZe.Domain;

public class Logradouro : Entity
{
    public string Cep { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;

    public Logradouro() { }

    public Logradouro(int id, string cep, string nome, string bairro, string cidade, string uf, string pais)
    {
        Id = id;
        Cep = cep;
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
        Pais = pais;
    }

    public static Result<Logradouro> Criar(int id, string cep, string nome, string bairro, string cidade, string uf, string pais)
    {
        var logradouro = new Logradouro(id, cep, nome, bairro, cidade, uf, pais);
        return Result<Logradouro>.Success(logradouro);
    }
}