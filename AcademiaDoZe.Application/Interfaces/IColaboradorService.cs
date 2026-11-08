using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IColaboradorService
{
    Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default);
    Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
