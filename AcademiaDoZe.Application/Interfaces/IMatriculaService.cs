using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IMatriculaService
{
    Task<IEnumerable<MatriculaDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default);
    Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
