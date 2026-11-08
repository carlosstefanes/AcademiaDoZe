using System;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    public static AlunoDto ToDto(this Aluno entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new AlunoDto
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
            Foto = entity.Foto != null ? new ArquivoDto { Conteudo = entity.Foto.Conteudo } : null
        };
    }

    public static Aluno ToEntity(this AlunoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var logradouro = dto.Endereco != null && dto.Endereco.Id > 0
            ? dto.Endereco.ToEntity()
            : new Logradouro { Id = dto.Endereco?.Id ?? 0 };
        var foto = dto.Foto?.Conteudo != null ? Arquivo.Criar(dto.Foto.Conteudo).Value : null;

        return Aluno.Criar(
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
            foto
        ).Value!;
    }

    public static Aluno UpdateFromDto(this Aluno entity, AlunoDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        var logradouro = dto.Endereco != null && dto.Endereco.Id > 0
            ? dto.Endereco.ToEntity()
            : new Logradouro { Id = dto.Endereco?.Id ?? 0 };
        var foto = dto.Foto?.Conteudo != null ? Arquivo.Criar(dto.Foto.Conteudo).Value : null;
        string senha = string.IsNullOrWhiteSpace(dto.Senha) ? entity.Senha.Valor : dto.Senha;

        return Aluno.Criar(
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
            foto
        ).Value!;
    }
}
