using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAlunoRepository : IRepository<Aluno>
{
    Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default);
    Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default);
    Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorCpf(string cpf, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorEmail(string email, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExiste(string cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExiste(string email, int? id = null, CancellationToken cancellationToken = default);
}
