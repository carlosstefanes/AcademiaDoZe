using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoService : IAcessoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoAlunoRepoFactory;
    private readonly Func<IAcessoColaboradorRepository> _acessoColaboradorRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

    public AcessoService(
        Func<IAcessoAlunoRepository> acessoAlunoRepoFactory,
        Func<IAcessoColaboradorRepository> acessoColaboradorRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _acessoAlunoRepoFactory = acessoAlunoRepoFactory;
        _acessoColaboradorRepoFactory = acessoColaboradorRepoFactory;
        _matriculaRepoFactory = matriculaRepoFactory;
        _alunoRepoFactory = alunoRepoFactory;
        _colaboradorRepoFactory = colaboradorRepoFactory;
    }

    public async Task<ResultadoAcessoDto> RegistrarEntradaAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var alunoRepo = _alunoRepoFactory();
        var matriculaRepo = _matriculaRepoFactory();
        var acessoAlunoRepo = _acessoAlunoRepoFactory();

        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken);
        string nomeAluno = aluno?.Nome ?? $"Aluno {alunoId}";

        // Regra: Validar se possui matrícula ativa
        var matriculaAtiva = await matriculaRepo.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        if (matriculaAtiva == null || matriculaAtiva.DataFim < DateOnly.FromDateTime(DateTime.Today))
        {
            return new ResultadoAcessoDto
            {
                Sucesso = false,
                Titulo = "Acesso Negado",
                Mensagem = "Acesso negado: o aluno não possui matrícula ativa.",
                Timestamp = DateTime.Now,
                NomePessoa = nomeAluno,
                TipoPessoa = "Aluno",
                EstaNaAcademia = false
            };
        }

        // Se já possui acesso em aberto, fecha ou mantém
        var acessoAberto = await acessoAlunoRepo.ObterUltimoAcessoAberto(alunoId, cancellationToken);
        if (acessoAberto != null)
        {
            // Fecha acesso anterior pendente
            acessoAberto.RegistrarSaida(DateTime.Now);
            await acessoAlunoRepo.Atualizar(acessoAberto, cancellationToken);
        }

        // Calcular quanto tempo ainda tem de plano
        int diasRestantes = (matriculaAtiva.DataFim.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
        if (diasRestantes < 0) diasRestantes = 0;

        var novoAcesso = AcessoAluno.Criar(0, alunoId, DateTime.Now).Value!;
        await acessoAlunoRepo.Adicionar(novoAcesso, cancellationToken);

        return new ResultadoAcessoDto
        {
            Sucesso = true,
            Titulo = "Entrada Autorizada",
            Mensagem = $"Entrada autorizada. Dias restantes de plano: {diasRestantes} dia(s).",
            Timestamp = DateTime.Now,
            NomePessoa = nomeAluno,
            TipoPessoa = "Aluno",
            EstaNaAcademia = true
        };
    }

    public async Task<ResultadoAcessoDto> RegistrarSaidaAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var alunoRepo = _alunoRepoFactory();
        var acessoAlunoRepo = _acessoAlunoRepoFactory();

        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken);
        string nomeAluno = aluno?.Nome ?? $"Aluno {alunoId}";

        var acessoAberto = await acessoAlunoRepo.ObterUltimoAcessoAberto(alunoId, cancellationToken);
        if (acessoAberto == null)
        {
            return new ResultadoAcessoDto
            {
                Sucesso = false,
                Titulo = "Atenção",
                Mensagem = "Nenhum registro de entrada em aberto encontrado para este aluno.",
                Timestamp = DateTime.Now,
                NomePessoa = nomeAluno,
                TipoPessoa = "Aluno",
                EstaNaAcademia = false
            };
        }

        var agora = DateTime.Now;
        acessoAberto.RegistrarSaida(agora);
        await acessoAlunoRepo.Atualizar(acessoAberto, cancellationToken);

        var permanencia = agora - acessoAberto.DataHoraEntrada;
        string tempoFormatado = permanencia.ToString(@"hh\:mm\:ss");

        return new ResultadoAcessoDto
        {
            Sucesso = true,
            Titulo = "Saída Registrada",
            Mensagem = $"Saída registrada. Tempo de permanência: {tempoFormatado}.",
            Timestamp = agora,
            NomePessoa = nomeAluno,
            TipoPessoa = "Aluno",
            EstaNaAcademia = false
        };
    }

    public async Task<ResultadoAcessoDto> RegistrarEntradaColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var colabRepo = _colaboradorRepoFactory();
        var acessoColabRepo = _acessoColaboradorRepoFactory();

        var colab = await colabRepo.ObterPorId(colaboradorId, cancellationToken);
        string nomeColab = colab?.Nome ?? $"Colaborador {colaboradorId}";

        var horasTrabalhadas = await acessoColabRepo.ObterHorasTrabalhadasHoje(colaboradorId, cancellationToken);

        // Regra: Validar se já não ultrapassa o limite de: 8 horas se for CLT, 6 horas se for estágio
        if (colab != null)
        {
            if (colab.Vinculo == ColaboradorVinculo.CLT && horasTrabalhadas.TotalHours >= 8)
            {
                return new ResultadoAcessoDto
                {
                    Sucesso = false,
                    Titulo = "Acesso Negado",
                    Mensagem = "Acesso negado: limite de 8 horas diárias para CLT já atingido hoje.",
                    Timestamp = DateTime.Now,
                    NomePessoa = nomeColab,
                    TipoPessoa = "Colaborador",
                    EstaNaAcademia = false
                };
            }
            if (colab.Vinculo == ColaboradorVinculo.Estagio && horasTrabalhadas.TotalHours >= 6)
            {
                return new ResultadoAcessoDto
                {
                    Sucesso = false,
                    Titulo = "Acesso Negado",
                    Mensagem = "Acesso negado: limite de 6 horas diárias para Estágio já atingido hoje.",
                    Timestamp = DateTime.Now,
                    NomePessoa = nomeColab,
                    TipoPessoa = "Colaborador",
                    EstaNaAcademia = false
                };
            }
        }

        var acessoAberto = await acessoColabRepo.ObterUltimoAcessoAberto(colaboradorId, cancellationToken);
        if (acessoAberto != null)
        {
            acessoAberto.RegistrarSaida(DateTime.Now);
            await acessoColabRepo.Atualizar(acessoAberto, cancellationToken);
            horasTrabalhadas = await acessoColabRepo.ObterHorasTrabalhadasHoje(colaboradorId, cancellationToken);
        }

        var novoAcesso = AcessoColaborador.Criar(0, colaboradorId, DateTime.Now).Value!;
        await acessoColabRepo.Adicionar(novoAcesso, cancellationToken);

        return new ResultadoAcessoDto
        {
            Sucesso = true,
            Titulo = "Entrada Autorizada",
            Mensagem = $"Entrada registrada. Horas trabalhadas hoje até o momento: {horasTrabalhadas:hh\\:mm\\:ss}.",
            Timestamp = DateTime.Now,
            NomePessoa = nomeColab,
            TipoPessoa = "Colaborador",
            EstaNaAcademia = true
        };
    }

    public async Task<ResultadoAcessoDto> RegistrarSaidaColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var colabRepo = _colaboradorRepoFactory();
        var acessoColabRepo = _acessoColaboradorRepoFactory();

        var colab = await colabRepo.ObterPorId(colaboradorId, cancellationToken);
        string nomeColab = colab?.Nome ?? $"Colaborador {colaboradorId}";

        var acessoAberto = await acessoColabRepo.ObterUltimoAcessoAberto(colaboradorId, cancellationToken);
        if (acessoAberto == null)
        {
            return new ResultadoAcessoDto
            {
                Sucesso = false,
                Titulo = "Atenção",
                Mensagem = "Nenhum registro de entrada em aberto encontrado para este colaborador.",
                Timestamp = DateTime.Now,
                NomePessoa = nomeColab,
                TipoPessoa = "Colaborador",
                EstaNaAcademia = false
            };
        }

        var agora = DateTime.Now;
        acessoAberto.RegistrarSaida(agora);
        await acessoColabRepo.Atualizar(acessoAberto, cancellationToken);

        var duracao = agora - acessoAberto.DataHoraEntrada;
        string permanenciaFormatada = duracao.ToString(@"hh\:mm\:ss");

        return new ResultadoAcessoDto
        {
            Sucesso = true,
            Titulo = "Saída Registrada",
            Mensagem = $"Saída registrada. Tempo de permanência: {permanenciaFormatada}.",
            Timestamp = agora,
            NomePessoa = nomeColab,
            TipoPessoa = "Colaborador",
            EstaNaAcademia = false
        };
    }

    public async Task<bool> AlunoEstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessoAlunoRepo = _acessoAlunoRepoFactory();
        var aberto = await acessoAlunoRepo.ObterUltimoAcessoAberto(alunoId, cancellationToken);
        return aberto != null;
    }

    public async Task<bool> ColaboradorEstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acessoColabRepo = _acessoColaboradorRepoFactory();
        var aberto = await acessoColabRepo.ObterUltimoAcessoAberto(colaboradorId, cancellationToken);
        return aberto != null;
    }

    public async Task<IEnumerable<AcessoHistoricoItemDto>> ObterTodosAcessosDeHojeAsync(CancellationToken cancellationToken = default)
    {
        var acessoAlunoRepo = _acessoAlunoRepoFactory();
        var acessoColabRepo = _acessoColaboradorRepoFactory();
        var matriculaRepo = _matriculaRepoFactory();

        var acessosAlunos = (await acessoAlunoRepo.ObterAcessosDeHoje(cancellationToken)).ToList();
        var acessosColabs = (await acessoColabRepo.ObterAcessosDeHoje(cancellationToken)).ToList();

        var itens = new List<AcessoHistoricoItemDto>();

        foreach (var a in acessosAlunos)
        {
            string nome = a.Aluno?.Nome ?? $"Aluno {a.AlunoId}";
            var foto = a.Aluno?.Foto != null ? new ArquivoDto { Conteudo = a.Aluno.Foto.Conteudo } : null;

            // Entrada
            int diasRestantes = 0;
            try
            {
                var mat = await matriculaRepo.ObterMatriculaAtivaPorAluno(a.AlunoId, cancellationToken);
                if (mat != null)
                {
                    diasRestantes = Math.Max(0, (mat.DataFim.ToDateTime(TimeOnly.MinValue) - a.DataHoraEntrada.Date).Days);
                }
            }
            catch { }

            itens.Add(new AcessoHistoricoItemDto
            {
                Id = a.Id,
                NomePessoa = nome,
                TipoPessoa = "Aluno",
                Descricao = $"Entrada autorizada. Dias restantes de plano: {diasRestantes} dia(s).",
                DataHora = a.DataHoraEntrada,
                IsEntrada = true,
                Foto = foto
            });

            // Saída se existir
            if (a.DataHoraSaida.HasValue)
            {
                var perm = a.DataHoraSaida.Value - a.DataHoraEntrada;
                itens.Add(new AcessoHistoricoItemDto
                {
                    Id = a.Id,
                    NomePessoa = nome,
                    TipoPessoa = "Aluno",
                    Descricao = $"Saída registrada. Tempo de permanência: {perm:hh\\:mm\\:ss}.",
                    DataHora = a.DataHoraSaida.Value,
                    IsEntrada = false,
                    Foto = foto
                });
            }
        }

        foreach (var c in acessosColabs)
        {
            string nome = c.Colaborador?.Nome ?? $"Colaborador {c.ColaboradorId}";
            var foto = c.Colaborador?.Foto != null ? new ArquivoDto { Conteudo = c.Colaborador.Foto.Conteudo } : null;

            itens.Add(new AcessoHistoricoItemDto
            {
                Id = c.Id,
                NomePessoa = nome,
                TipoPessoa = "Colaborador",
                Descricao = "Entrada registrada. Horas trabalhadas hoje até o momento: 00:00:00.",
                DataHora = c.DataHoraEntrada,
                IsEntrada = true,
                Foto = foto
            });

            if (c.DataHoraSaida.HasValue)
            {
                var perm = c.DataHoraSaida.Value - c.DataHoraEntrada;
                itens.Add(new AcessoHistoricoItemDto
                {
                    Id = c.Id,
                    NomePessoa = nome,
                    TipoPessoa = "Colaborador",
                    Descricao = $"Saída registrada. Tempo de permanência: {perm:hh\\:mm\\:ss}.",
                    DataHora = c.DataHoraSaida.Value,
                    IsEntrada = false,
                    Foto = foto
                });
            }
        }

        return itens.OrderByDescending(x => x.DataHora);
    }
}
