using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoService
{
    Task<ResultadoAcessoDto> RegistrarEntradaAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<ResultadoAcessoDto> RegistrarSaidaAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<ResultadoAcessoDto> RegistrarEntradaColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<ResultadoAcessoDto> RegistrarSaidaColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);

    Task<bool> AlunoEstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> ColaboradorEstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AcessoHistoricoItemDto>> ObterTodosAcessosDeHojeAsync(CancellationToken cancellationToken = default);
}
