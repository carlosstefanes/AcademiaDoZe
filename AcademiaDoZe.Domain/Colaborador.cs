namespace AcademiaDoZe.Domain;

public class Colaborador : Entity
{
    public Cpf Cpf { get; private set; }
    public string Nome { get; private set; }
    public DateOnly DataNascimento { get; private set; }
    public Telefone Telefone { get; private set; }
    public Email Email { get; private set; }
    public Endereco Endereco { get; private set; }
    public Senha Senha { get; private set; }
    public Arquivo? Foto { get; private set; }
    public DateOnly DataAdmissao { get; private set; }
    public ColaboradorTipo Tipo { get; private set; }
    public ColaboradorVinculo Vinculo { get; private set; }

    private Colaborador(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email, Endereco endereco, Senha senha, Arquivo? foto, DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        Id = id;
        Nome = nome;
        Cpf = cpf;
        DataNascimento = dataNascimento;
        Telefone = telefone;
        Email = email;
        Endereco = endereco;
        Senha = senha;
        Foto = foto;
        DataAdmissao = dataAdmissao;
        Tipo = tipo;
        Vinculo = vinculo;
    }

    public static Result<Colaborador> Criar(int id, string nome, string cpf, DateOnly dataNascimento, string telefone, string email, Logradouro endereco, string numero, string? complemento, string senha, Arquivo? foto, DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var cpfVo = Cpf.Criar(cpf);
        var telVo = Telefone.Criar(telefone);
        var emailVo = Email.Criar(email);
        var senhaVo = Senha.Criar(senha);
        var endVo = Endereco.Criar(endereco.Id, numero, complemento);

        var colaborador = new Colaborador(id, nome, cpfVo.Value!, dataNascimento, telVo.Value!, emailVo.Value!, endVo.Value!, senhaVo.Value!, foto, dataAdmissao, tipo, vinculo);
        return Result<Colaborador>.Success(colaborador);
    }
}