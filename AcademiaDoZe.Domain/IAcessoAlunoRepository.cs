using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoAlunoRepository : IRepository<AcessoAluno>
{
    Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAluno>> ObterPorAlunoId(int alunoId, CancellationToken cancellationToken = default);
    Task<AcessoAluno?> ObterUltimoAcessoAberto(int alunoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAluno>> ObterAcessosDeHoje(CancellationToken cancellationToken = default);
    Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default);
    Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}
