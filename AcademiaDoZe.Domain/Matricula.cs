namespace AcademiaDoZe.Domain;

public class Matricula : Entity
{
    public int AlunoId { get; private set; }
    public Aluno Aluno { get; private set; } = null!;
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; } = string.Empty;
    public MatriculaRestricoes RestricoesMedicas { get; private set; }
    public string? ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula() { }

    private Matricula(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico, string? observacoesRestricoes)
    {
        Id = id;
        Aluno = aluno;
        AlunoId = aluno?.Id ?? 0;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = CalcularDataFim(dataInicio, plano);
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        LaudoMedico = laudoMedico;
        ObservacoesRestricoes = observacoesRestricoes;
    }

    public static Result<Matricula> Criar(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico = null, string? observacoesRestricoes = null)
    {
        if (aluno == null)
            return Result<Matricula>.Failure("O aluno é obrigatório.");

        if (string.IsNullOrWhiteSpace(objetivo))
            return Result<Matricula>.Failure("O objetivo é obrigatório.");

        var matricula = new Matricula(id, aluno, plano, dataInicio, objetivo, restricoesMedicas, laudoMedico, observacoesRestricoes);
        return Result<Matricula>.Success(matricula);
    }

    public static DateOnly CalcularDataFim(DateOnly dataInicio, MatriculaPlano plano)
    {
        return plano switch
        {
            MatriculaPlano.Mensal => dataInicio.AddMonths(1),
            MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
            MatriculaPlano.Semestral => dataInicio.AddMonths(6),
            MatriculaPlano.Anual => dataInicio.AddYears(1),
            _ => dataInicio.AddMonths(1)
        };
    }
}