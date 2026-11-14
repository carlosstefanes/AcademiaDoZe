using System;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public int AlunoId { get; private set; }
    public Aluno? Aluno { get; private set; }
    public DateTime DataHoraEntrada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    public bool EstaNaAcademia => !DataHoraSaida.HasValue;
    public TimeSpan? TempoPermanencia => DataHoraSaida.HasValue ? DataHoraSaida.Value - DataHoraEntrada : null;

    private AcessoAluno(int id, int alunoId, DateTime dataHoraEntrada, DateTime? dataHoraSaida, Aluno? aluno = null)
    {
        Id = id;
        AlunoId = alunoId;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
        Aluno = aluno;
    }

    public static Result<AcessoAluno> Criar(int id, int alunoId, DateTime dataHoraEntrada, DateTime? dataHoraSaida = null, Aluno? aluno = null)
    {
        if (alunoId <= 0)
            return Result<AcessoAluno>.Failure("O ID do aluno é obrigatório.");

        if (dataHoraSaida.HasValue && dataHoraSaida.Value < dataHoraEntrada)
            return Result<AcessoAluno>.Failure("A data/hora de saída não pode ser anterior à de entrada.");

        var acesso = new AcessoAluno(id, alunoId, dataHoraEntrada, dataHoraSaida, aluno);
        return Result<AcessoAluno>.Success(acesso);
    }

    public Result<bool> RegistrarSaida(DateTime dataHoraSaida)
    {
        if (dataHoraSaida < DataHoraEntrada)
            return Result<bool>.Failure("A data/hora de saída não pode ser anterior à data/hora de entrada.");

        DataHoraSaida = dataHoraSaida;
        return Result<bool>.Success(true);
    }
}
