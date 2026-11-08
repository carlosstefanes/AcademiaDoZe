using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default);
    Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default);
    Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorCpf(string cpf, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorEmail(string email, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExiste(string cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExiste(string email, int? id = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default);
}
