using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAcessoService _acessoService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;

    [ObservableProperty]
    private bool _isAlunoMode = true;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public bool IsColaboradorMode => !IsAlunoMode;
    public string ModoAtualTexto => IsAlunoMode ? "Modo atual selecionado: Aluno" : "Modo atual selecionado: Colaborador";
    public string PessoasLabel => IsAlunoMode ? "Selecione o Aluno:" : "Selecione o Colaborador:";
    public string PickerPlaceholder => IsAlunoMode ? "Toque para escolher o aluno..." : "Toque para escolher o colaborador...";

    [ObservableProperty]
    private PessoaDto? _selectedPessoa;

    [ObservableProperty]
    private bool _estaNaAcademia;

    [ObservableProperty]
    private bool _feedbackVisivel;

    [ObservableProperty]
    private bool _feedbackSucesso;

    [ObservableProperty]
    private string _feedbackTitulo = string.Empty;

    [ObservableProperty]
    private string _feedbackMensagem = string.Empty;

    public bool TemPessoaSelecionada => SelectedPessoa != null && SelectedPessoa.Id > 0;
    public string StatusTexto => EstaNaAcademia ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public string BotaoAcaoTexto => EstaNaAcademia ? "Registrar Saída" : "Registrar Entrada";

    public ObservableCollection<PessoaDto> PessoasDisponiveis { get; } = [];
    public ObservableCollection<AcessoHistoricoItemDto> AcessosHoje { get; } = [];

    private List<AlunoDto> _todosAlunos = [];
    private List<ColaboradorDto> _todosColaboradores = [];

    public AcessoViewModel(
        IAcessoService acessoService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService)
    {
        _acessoService = acessoService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        Title = "Controle de Acesso";
    }

    public async Task InitializeAsync()
    {
        await CarregarDadosAsync();
        await AtualizarHistoricoAsync();
    }

    [RelayCommand]
    public async Task SelecionarModoAlunoAsync()
    {
        if (IsAlunoMode) return;
        IsAlunoMode = true;
        OnPropertyChanged(nameof(IsColaboradorMode));
        OnPropertyChanged(nameof(ModoAtualTexto));
        OnPropertyChanged(nameof(PessoasLabel));
        OnPropertyChanged(nameof(PickerPlaceholder));
        SelectedPessoa = null;
        FeedbackVisivel = false;
        FiltrarPessoas();
        await AtualizarStatusPessoaAsync();
    }

    [RelayCommand]
    public async Task SelecionarModoColaboradorAsync()
    {
        if (!IsAlunoMode) return;
        IsAlunoMode = false;
        OnPropertyChanged(nameof(IsColaboradorMode));
        OnPropertyChanged(nameof(ModoAtualTexto));
        OnPropertyChanged(nameof(PessoasLabel));
        OnPropertyChanged(nameof(PickerPlaceholder));
        SelectedPessoa = null;
        FeedbackVisivel = false;
        FiltrarPessoas();
        await AtualizarStatusPessoaAsync();
    }

    [RelayCommand]
    public async Task CarregarDadosAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var alunos = await _alunoService.ObterTodosAsync(cts.Token);
            _todosAlunos = (alunos ?? []).ToList();

            var colabs = await _colaboradorService.ObterTodosAsync(cts.Token);
            _todosColaboradores = (colabs ?? []).ToList();

            FiltrarPessoas();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar pessoas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void BuscarPessoa()
    {
        FiltrarPessoas();
    }

    partial void OnSearchTextChanged(string value)
    {
        FiltrarPessoas();
    }

    partial void OnSelectedPessoaChanged(PessoaDto? value)
    {
        OnPropertyChanged(nameof(TemPessoaSelecionada));
        _ = AtualizarStatusPessoaAsync();
    }

    private void FiltrarPessoas()
    {
        PessoasDisponiveis.Clear();
        string termo = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

        if (IsAlunoMode)
        {
            var query = _todosAlunos.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(termo))
            {
                query = query.Where(a =>
                    (a.Nome?.ToLowerInvariant().Contains(termo) ?? false) ||
                    (a.Cpf?.Contains(termo) ?? false) ||
                    a.Id.ToString() == termo);
            }
            foreach (var a in query)
            {
                PessoasDisponiveis.Add(a);
            }

            if (SelectedPessoa == null && PessoasDisponiveis.Count > 0)
            {
                SelectedPessoa = PessoasDisponiveis[0];
            }
        }
        else
        {
            var query = _todosColaboradores.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(termo))
            {
                query = query.Where(c =>
                    (c.Nome?.ToLowerInvariant().Contains(termo) ?? false) ||
                    (c.Cpf?.Contains(termo) ?? false) ||
                    c.Id.ToString() == termo);
            }
            foreach (var c in query)
            {
                PessoasDisponiveis.Add(c);
            }

            if (SelectedPessoa == null && PessoasDisponiveis.Count > 0)
            {
                SelectedPessoa = PessoasDisponiveis[0];
            }
        }
    }

    private async Task AtualizarStatusPessoaAsync()
    {
        if (SelectedPessoa == null || SelectedPessoa.Id <= 0)
        {
            EstaNaAcademia = false;
            OnPropertyChanged(nameof(StatusTexto));
            OnPropertyChanged(nameof(BotaoAcaoTexto));
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (IsAlunoMode)
            {
                EstaNaAcademia = await _acessoService.AlunoEstaNaAcademiaAsync(SelectedPessoa.Id, cts.Token);
            }
            else
            {
                EstaNaAcademia = await _acessoService.ColaboradorEstaNaAcademiaAsync(SelectedPessoa.Id, cts.Token);
            }
        }
        catch
        {
            EstaNaAcademia = false;
        }

        OnPropertyChanged(nameof(StatusTexto));
        OnPropertyChanged(nameof(BotaoAcaoTexto));
    }

    [RelayCommand]
    public async Task RegistrarAcessoAsync()
    {
        if (SelectedPessoa == null || SelectedPessoa.Id <= 0)
        {
            FeedbackTitulo = "Aviso";
            FeedbackMensagem = "Por favor, selecione uma pessoa para registrar acesso.";
            FeedbackSucesso = false;
            FeedbackVisivel = true;
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            ResultadoAcessoDto resultado;

            if (IsAlunoMode)
            {
                if (EstaNaAcademia)
                {
                    resultado = await _acessoService.RegistrarSaidaAlunoAsync(SelectedPessoa.Id, cts.Token);
                }
                else
                {
                    resultado = await _acessoService.RegistrarEntradaAlunoAsync(SelectedPessoa.Id, cts.Token);
                }
            }
            else
            {
                if (EstaNaAcademia)
                {
                    resultado = await _acessoService.RegistrarSaidaColaboradorAsync(SelectedPessoa.Id, cts.Token);
                }
                else
                {
                    resultado = await _acessoService.RegistrarEntradaColaboradorAsync(SelectedPessoa.Id, cts.Token);
                }
            }

            FeedbackTitulo = resultado.Titulo;
            FeedbackMensagem = resultado.Mensagem;
            FeedbackSucesso = resultado.Sucesso;
            FeedbackVisivel = true;

            if (resultado.Sucesso)
            {
                EstaNaAcademia = resultado.EstaNaAcademia;
                OnPropertyChanged(nameof(StatusTexto));
                OnPropertyChanged(nameof(BotaoAcaoTexto));
            }

            await AtualizarHistoricoAsync();
        }
        catch (Exception ex)
        {
            FeedbackTitulo = "Erro";
            FeedbackMensagem = $"Erro ao registrar acesso: {ex.Message}";
            FeedbackSucesso = false;
            FeedbackVisivel = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AtualizarHistoricoAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var historico = await _acessoService.ObterTodosAcessosDeHojeAsync(cts.Token);

            AcessosHoje.Clear();
            foreach (var item in historico)
            {
                AcessosHoje.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar histórico: {ex.Message}");
        }
    }
}
