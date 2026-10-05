namespace AcademiaDoZe.Domain;

public class Aluno : Entity
{
    public Cpf Cpf { get; private set; }
    public string Nome { get; private set; }
    public DateOnly DataNascimento { get; private set; }
    public Telefone Telefone { get; private set; }
    public Email Email { get; private set; }
    public Endereco Endereco { get; private set; }
    public Senha Senha { get; private set; }
    public Arquivo? Foto { get; private set; }

    private Aluno(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email, Endereco endereco, Senha senha, Arquivo? foto)
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
    }

    public static Result<Aluno> Criar(int id, string nome, string cpf, DateOnly dataNascimento, string telefone, string email, Logradouro endereco, string numero, string? complemento, string senha, Arquivo? foto)
    {
        var cpfVo = Cpf.Criar(cpf);
        var telVo = Telefone.Criar(telefone);
        var emailVo = Email.Criar(email);
        var senhaVo = Senha.Criar(senha);
        var endVo = Endereco.Criar(endereco.Id, numero, complemento);

        var aluno = new Aluno(id, nome, cpfVo.Value!, dataNascimento, telVo.Value!, emailVo.Value!, endVo.Value!, senhaVo.Value!, foto);
        return Result<Aluno>.Success(aluno);
    }
}