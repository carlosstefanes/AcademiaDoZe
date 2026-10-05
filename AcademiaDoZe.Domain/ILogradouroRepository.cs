namespace AcademiaDoZe.Domain;

public interface ILogradouroRepository : IRepository<Logradouro>
{
    Task<Logradouro?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Logradouro> InserirAsync(Logradouro entity, CancellationToken cancellationToken = default);
    Task<Logradouro> AtualizarAsync(Logradouro entity, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}