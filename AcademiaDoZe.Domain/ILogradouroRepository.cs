using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Repositories;

public interface ILogradouroRepository : IRepository<Logradouro>
{
    Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default);
    Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default);
    Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
    Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default);
    Task<bool> CepJaExiste(Cep cep, int? idDesconsiderar = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default);

    Task<Logradouro?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) => ObterPorId(id, cancellationToken);
    Task<IEnumerable<Logradouro>> ObterTodosAsync(CancellationToken cancellationToken = default) => ObterTodos(cancellationToken);
    Task<Logradouro> InserirAsync(Logradouro entity, CancellationToken cancellationToken = default) => Adicionar(entity, cancellationToken);
    Task<Logradouro> AtualizarAsync(Logradouro entity, CancellationToken cancellationToken = default) => Atualizar(entity, cancellationToken);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default) => Remover(id, cancellationToken);
}
