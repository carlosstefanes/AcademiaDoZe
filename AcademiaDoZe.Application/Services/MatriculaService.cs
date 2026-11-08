using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _repoFactory;

    public MatriculaService(Func<IMatriculaRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var matriculas = await _repoFactory().ObterTodos(cancellationToken);
        return [.. matriculas.Select(m => m.ToDto())];
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _repoFactory().ObterPorId(id, cancellationToken);
        return matricula?.ToDto();
    }

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);

        if (await _repoFactory().PossuiMatriculaAtiva(matriculaDto.AlunoMatricula.Id, cancellationToken))
            throw new InvalidOperationException("O aluno já possui uma matrícula ativa.");

        var matricula = matriculaDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(matricula, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);

        var matriculaExistente = await _repoFactory().ObterPorId(matriculaDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Matrícula com ID {matriculaDto.Id} não encontrada.");

        var matriculaAtualizada = matriculaExistente.UpdateFromDto(matriculaDto);
        var atualizado = await _repoFactory().Atualizar(matriculaAtualizada, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _repoFactory().ObterPorId(id, cancellationToken);
        if (matricula == null) return false;
        return await _repoFactory().Remover(id, cancellationToken);
    }
}

