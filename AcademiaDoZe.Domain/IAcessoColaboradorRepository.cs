using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoColaboradorRepository : IRepository<AcessoColaborador>
{
    Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaborador>> ObterPorColaboradorId(int colaboradorId, CancellationToken cancellationToken = default);
    Task<AcessoColaborador?> ObterUltimoAcessoAberto(int colaboradorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaborador>> ObterAcessosDeHoje(CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterHorasTrabalhadasHoje(int colaboradorId, CancellationToken cancellationToken = default);
    Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default);
    Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}
