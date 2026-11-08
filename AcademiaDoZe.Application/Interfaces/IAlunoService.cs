using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAlunoService
{
    Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default);
    Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
}
