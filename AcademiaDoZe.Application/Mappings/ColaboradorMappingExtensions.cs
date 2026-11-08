using System;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new ColaboradorDto
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.Cpf.Valor,
            DataNascimento = entity.DataNascimento,
            Telefone = entity.Telefone.Valor,
            Email = entity.Email.Valor,
            Endereco = new LogradouroDto { Id = entity.Endereco?.LogradouroId ?? 0 },
            Numero = entity.Endereco?.Numero ?? string.Empty,
            Complemento = entity.Endereco?.Complemento,
            Foto = entity.Foto != null ? new ArquivoDto { Conteudo = entity.Foto.Conteudo } : null,
            DataAdmissao = entity.DataAdmissao,
            Tipo = entity.Tipo.ToApplication(),
            Vinculo = entity.Vinculo.ToApplication()
        };
    }

    public static Colaborador ToEntity(this ColaboradorDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var logradouro = dto.Endereco != null && dto.Endereco.Id > 0
            ? dto.Endereco.ToEntity()
            : new Logradouro { Id = dto.Endereco?.Id ?? 0 };
        var foto = dto.Foto?.Conteudo != null ? Arquivo.Criar(dto.Foto.Conteudo).Value : null;

        return Colaborador.Criar(
            dto.Id,
            dto.Nome,
            dto.Cpf,
            dto.DataNascimento,
            dto.Telefone,
            dto.Email,
            logradouro,
            dto.Numero,
            dto.Complemento,
            dto.Senha,
            foto,
            dto.DataAdmissao,
            dto.Tipo.ToDomain(),
            dto.Vinculo.ToDomain()
        ).Value!;
    }

    public static Colaborador UpdateFromDto(this Colaborador entity, ColaboradorDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        var logradouro = dto.Endereco != null && dto.Endereco.Id > 0
            ? dto.Endereco.ToEntity()
            : new Logradouro { Id = dto.Endereco?.Id ?? 0 };
        var foto = dto.Foto?.Conteudo != null ? Arquivo.Criar(dto.Foto.Conteudo).Value : null;
        string senha = string.IsNullOrWhiteSpace(dto.Senha) ? entity.Senha.Valor : dto.Senha;

        return Colaborador.Criar(
            dto.Id,
            dto.Nome,
            dto.Cpf,
            dto.DataNascimento,
            dto.Telefone,
            dto.Email,
            logradouro,
            dto.Numero,
            dto.Complemento,
            senha,
            foto,
            dto.DataAdmissao,
            dto.Tipo.ToDomain(),
            dto.Vinculo.ToDomain()
        ).Value!;
    }
}
