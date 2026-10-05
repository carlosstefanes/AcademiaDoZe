namespace AcademiaDoZe.Domain;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default);
    Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default);
    Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}