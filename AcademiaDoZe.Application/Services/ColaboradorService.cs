using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    private readonly Func<IColaboradorRepository> _repoFactory;

    public ColaboradorService(Func<IColaboradorRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterTodos(cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorCpf(cpf, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorEmail(email, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        var domainTipo = tipo.ToDomain();
        var colaboradores = await _repoFactory().ObterPorTipo(domainTipo, cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        var domainVinculo = vinculo.ToDomain();
        var colaboradores = await _repoFactory().ObterPorVinculo(domainVinculo, cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException($"CPF inválido: {string.Join(",", cpfResult.Notifications.Select(n => n.Mensagem))}");

        var emailResult = Email.Criar(colaboradorDto.Email);
        if (emailResult.IsFailure)
            throw new ArgumentException($"E-mail inválido: {string.Join(",", emailResult.Notifications.Select(n => n.Mensagem))}");

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!.Valor, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailResult.Value!.Valor, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um colaborador cadastrado com o e-mail {colaboradorDto.Email}.");

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
            colaboradorDto.Senha = PasswordHasher.Hash(colaboradorDto.Senha);

        var colaborador = colaboradorDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(colaborador, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var colaboradorExistente = await _repoFactory().ObterPorId(colaboradorDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Colaborador com ID {colaboradorDto.Id} não encontrado.");

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException("CPF inválido.");

        var emailResult = Email.Criar(colaboradorDto.Email);
        if (emailResult.IsFailure)
            throw new ArgumentException("E-mail inválido.");

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!.Valor, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailResult.Value!.Valor, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o e-mail {colaboradorDto.Email}.");

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
            colaboradorDto.Senha = PasswordHasher.Hash(colaboradorDto.Senha);
        else
            colaboradorDto.Senha = colaboradorExistente.Senha.Valor;

        var colaboradorAtualizado = colaboradorExistente.UpdateFromDto(colaboradorDto);
        var atualizado = await _repoFactory().Atualizar(colaboradorAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        if (colaborador == null) return false;
        return await _repoFactory().Remover(id, cancellationToken);
    }
}
