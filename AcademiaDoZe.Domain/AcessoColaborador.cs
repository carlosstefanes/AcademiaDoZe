using System;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public int ColaboradorId { get; private set; }
    public Colaborador? Colaborador { get; private set; }
    public DateTime DataHoraEntrada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    public bool EstaNaAcademia => !DataHoraSaida.HasValue;
    public TimeSpan? TempoPermanencia => DataHoraSaida.HasValue ? DataHoraSaida.Value - DataHoraEntrada : null;

    private AcessoColaborador(int id, int colaboradorId, DateTime dataHoraEntrada, DateTime? dataHoraSaida, Colaborador? colaborador = null)
    {
        Id = id;
        ColaboradorId = colaboradorId;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
        Colaborador = colaborador;
    }

    public static Result<AcessoColaborador> Criar(int id, int colaboradorId, DateTime dataHoraEntrada, DateTime? dataHoraSaida = null, Colaborador? colaborador = null)
    {
        if (colaboradorId <= 0)
            return Result<AcessoColaborador>.Failure("O ID do colaborador é obrigatório.");

        if (dataHoraSaida.HasValue && dataHoraSaida.Value < dataHoraEntrada)
            return Result<AcessoColaborador>.Failure("A data/hora de saída não pode ser anterior à de entrada.");

        var acesso = new AcessoColaborador(id, colaboradorId, dataHoraEntrada, dataHoraSaida, colaborador);
        return Result<AcessoColaborador>.Success(acesso);
    }

    public Result<bool> RegistrarSaida(DateTime dataHoraSaida)
    {
        if (dataHoraSaida < DataHoraEntrada)
            return Result<bool>.Failure("A data/hora de saída não pode ser anterior à data/hora de entrada.");

        DataHoraSaida = dataHoraSaida;
        return Result<bool>.Success(true);
    }
}
