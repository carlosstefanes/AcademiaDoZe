using System;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new LogradouroDto
        {
            Id = entity.Id,
            Cep = entity.Cep,
            Nome = entity.Nome,
            Bairro = entity.Bairro,
            Cidade = entity.Cidade,
            Estado = entity.Uf,
            Pais = entity.Pais
        };
    }

    public static Logradouro ToEntity(this LogradouroDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Logradouro.Criar(
            dto.Id,
            dto.Cep,
            dto.Nome,
            dto.Bairro,
            dto.Cidade,
            dto.Estado,
            dto.Pais
        ).Value!;
    }

    public static Logradouro UpdateFromDto(this Logradouro entity, LogradouroDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        entity.Cep = dto.Cep;
        entity.Nome = dto.Nome;
        entity.Bairro = dto.Bairro;
        entity.Cidade = dto.Cidade;
        entity.Uf = dto.Estado;
        entity.Pais = dto.Pais;

        return entity;
    }
}

