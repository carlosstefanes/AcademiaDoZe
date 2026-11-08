using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly Func<IAlunoRepository> _repoFactory;

    public AlunoService(Func<IAlunoRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var alunos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. alunos.Select(a => a.ToDto())];
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException($"CPF inválido: {string.Join(",", cpfResult.Notifications.Select(n => n.Mensagem))}");

        var emailResult = Email.Criar(alunoDto.Email);
        if (emailResult.IsFailure)
            throw new ArgumentException($"E-mail inválido: {string.Join(",", emailResult.Notifications.Select(n => n.Mensagem))}");

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!.Valor, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um aluno cadastrado com o CPF {alunoDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailResult.Value!.Valor, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um aluno cadastrado com o e-mail {alunoDto.Email}.");

        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
            alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);

        var aluno = alunoDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(aluno, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var alunoExistente = await _repoFactory().ObterPorId(alunoDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Aluno com ID {alunoDto.Id} não encontrado.");

        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException("CPF inválido.");

        var emailResult = Email.Criar(alunoDto.Email);
        if (emailResult.IsFailure)
            throw new ArgumentException("E-mail inválido.");

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!.Valor, alunoDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro aluno cadastrado com o CPF {alunoDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailResult.Value!.Valor, alunoDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro aluno cadastrado com o e-mail {alunoDto.Email}.");

        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
            alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);
        else
            alunoDto.Senha = alunoExistente.Senha.Valor;

        var alunoAtualizado = alunoExistente.UpdateFromDto(alunoDto);
        var atualizado = await _repoFactory().Atualizar(alunoAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        if (aluno == null) return false;
        return await _repoFactory().Remover(id, cancellationToken);
    }
}

