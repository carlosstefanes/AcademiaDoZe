using System;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new MatriculaDto
        {
            Id = entity.Id,
            AlunoMatricula = entity.Aluno?.ToDto() ?? new AlunoDto { Id = entity.AlunoId },
            Plano = entity.Plano.ToApplication(),
            DataInicio = entity.DataInicio,
            DataFim = entity.DataFim,
            Objetivo = entity.Objetivo,
            RestricoesMedicas = entity.RestricoesMedicas.ToApplication(),
            ObservacoesRestricoes = entity.ObservacoesRestricoes,
            LaudoMedico = entity.LaudoMedico != null ? new ArquivoDto { Conteudo = entity.LaudoMedico.Conteudo } : null
        };
    }

    public static Matricula ToEntity(this MatriculaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var aluno = dto.AlunoMatricula.ToEntity();
        var laudo = dto.LaudoMedico?.Conteudo != null ? Arquivo.Criar(dto.LaudoMedico.Conteudo).Value : null;

        return Matricula.Criar(
            dto.Id,
            aluno,
            dto.Plano.ToDomain(),
            dto.DataInicio,
            dto.Objetivo,
            dto.RestricoesMedicas.ToDomain(),
            laudo,
            dto.ObservacoesRestricoes
        ).Value!;
    }

    public static Matricula UpdateFromDto(this Matricula entity, MatriculaDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        var aluno = dto.AlunoMatricula != null ? dto.AlunoMatricula.ToEntity() : entity.Aluno;
        var laudo = dto.LaudoMedico?.Conteudo != null ? Arquivo.Criar(dto.LaudoMedico.Conteudo).Value : entity.LaudoMedico;

        return Matricula.Criar(
            dto.Id,
            aluno,
            dto.Plano.ToDomain(),
            dto.DataInicio,
            dto.Objetivo,
            dto.RestricoesMedicas.ToDomain(),
            laudo,
            dto.ObservacoesRestricoes
        ).Value!;
    }
}

