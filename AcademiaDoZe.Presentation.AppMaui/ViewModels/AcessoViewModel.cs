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

    public Color BadgeBackgroundColor => EstaNaAcademia ? Color.FromArgb("#065F46") : Color.FromArgb("#1E293B");
    public Color BadgeTextColor => EstaNaAcademia ? Color.FromArgb("#34D399") : Color.FromArgb("#CBD5E1");
    public Color FeedbackBackgroundColor => FeedbackSucesso ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
    public Color FeedbackBorderColor => FeedbackSucesso ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
    public string FeedbackIcon => FeedbackSucesso ? "\ue86c" : "\ue000";
    public Color AlunoButtonColor => IsAlunoMode ? Color.FromArgb("#512BD4") : Color.FromArgb("#334155");
    public Color AlunoButtonTextColor => IsAlunoMode ? Colors.White : Color.FromArgb("#94A3B8");
    public Color ColaboradorButtonColor => !IsAlunoMode ? Color.FromArgb("#512BD4") : Color.FromArgb("#334155");
    public Color ColaboradorButtonTextColor => !IsAlunoMode ? Colors.White : Color.FromArgb("#94A3B8");

    public bool TemAcessosHoje => AcessosHoje.Count > 0;
    public bool NaoTemAcessosHoje => AcessosHoje.Count == 0;

    public void NotificarVisual()
    {
        OnPropertyChanged(nameof(TemPessoaSelecionada));
        OnPropertyChanged(nameof(StatusTexto));
        OnPropertyChanged(nameof(BotaoAcaoTexto));
        OnPropertyChanged(nameof(BadgeBackgroundColor));
        OnPropertyChanged(nameof(BadgeTextColor));
        OnPropertyChanged(nameof(FeedbackBackgroundColor));
        OnPropertyChanged(nameof(FeedbackBorderColor));
        OnPropertyChanged(nameof(FeedbackIcon));
        OnPropertyChanged(nameof(AlunoButtonColor));
        OnPropertyChanged(nameof(AlunoButtonTextColor));
        OnPropertyChanged(nameof(ColaboradorButtonColor));
        OnPropertyChanged(nameof(ColaboradorButtonTextColor));
        OnPropertyChanged(nameof(ModoAtualTexto));
        OnPropertyChanged(nameof(PessoasLabel));
        OnPropertyChanged(nameof(PickerPlaceholder));
        OnPropertyChanged(nameof(TemAcessosHoje));
        OnPropertyChanged(nameof(NaoTemAcessosHoje));
    }

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
        SelectedPessoa = null;
        FeedbackVisivel = false;
        NotificarVisual();
        FiltrarPessoas();
        await AtualizarStatusPessoaAsync();
    }

    [RelayCommand]
    public async Task SelecionarModoColaboradorAsync()
    {
        if (!IsAlunoMode) return;
        IsAlunoMode = false;
        SelectedPessoa = null;
        FeedbackVisivel = false;
        NotificarVisual();
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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            string termo = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
            int currentSelectedId = SelectedPessoa?.Id ?? 0;

            var novaLista = new List<PessoaDto>();
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
                novaLista.AddRange(query);
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
                novaLista.AddRange(query);
            }

            PessoasDisponiveis.Clear();
            foreach (var p in novaLista)
            {
                PessoasDisponiveis.Add(p);
            }

            SelectedPessoa = PessoasDisponiveis.FirstOrDefault(p => p.Id == currentSelectedId)
                             ?? PessoasDisponiveis.FirstOrDefault();

            NotificarVisual();
        });
    }

    private async Task AtualizarStatusPessoaAsync()
    {
        var pessoa = SelectedPessoa;
        if (pessoa == null || pessoa.Id <= 0)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EstaNaAcademia = false;
                NotificarVisual();
            });
            return;
        }

        bool dentro = false;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (IsAlunoMode)
            {
                dentro = await _acessoService.AlunoEstaNaAcademiaAsync(pessoa.Id, cts.Token);
            }
            else
            {
                dentro = await _acessoService.ColaboradorEstaNaAcademiaAsync(pessoa.Id, cts.Token);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AcessoViewModel] Erro ao verificar status: {ex.Message}");
            dentro = false;
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (SelectedPessoa?.Id == pessoa.Id)
            {
                EstaNaAcademia = dentro;
                NotificarVisual();
            }
        });
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
            NotificarVisual();
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
            }
            NotificarVisual();

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
            var historico = (await _acessoService.ObterTodosAcessosDeHojeAsync(cts.Token)).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AcessosHoje.Clear();
                foreach (var item in historico)
                {
                    AcessosHoje.Add(item);
                }
                OnPropertyChanged(nameof(TemAcessosHoje));
                OnPropertyChanged(nameof(NaoTemAcessosHoje));
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar histórico: {ex.Message}");
        }
    }
}
