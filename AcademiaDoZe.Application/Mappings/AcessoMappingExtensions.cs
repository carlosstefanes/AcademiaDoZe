using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class AcessoMappingExtensions
{
    public static AcessoAlunoDto ToDto(this AcessoAluno entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new AcessoAlunoDto
        {
            Id = entity.Id,
            AlunoId = entity.AlunoId,
            Aluno = entity.Aluno?.ToDto(),
            DataHoraEntrada = entity.DataHoraEntrada,
            DataHoraSaida = entity.DataHoraSaida
        };
    }

    public static AcessoColaboradorDto ToDto(this AcessoColaborador entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new AcessoColaboradorDto
        {
            Id = entity.Id,
            ColaboradorId = entity.ColaboradorId,
            Colaborador = entity.Colaborador?.ToDto(),
            DataHoraEntrada = entity.DataHoraEntrada,
            DataHoraSaida = entity.DataHoraSaida
        };
    }
}
