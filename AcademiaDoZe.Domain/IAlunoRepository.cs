namespace AcademiaDoZe.Domain;

public interface IAlunoRepository : IRepository<Aluno>
{
    Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default);
    Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default);
    Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}