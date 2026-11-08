using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly List<LogradouroDto> _allLogradouros = new();

    public ObservableCollection<LogradouroDto> Logradouros { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilterOption = "Cidade";

    public List<string> FilterOptions { get; } = new() { "Cidade", "ID", "CEP" };

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterLogradouros();
    }

    partial void OnSelectedFilterOptionChanged(string value)
    {
        FilterLogradouros();
    }

    [RelayCommand]
    public async Task LoadLogradourosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsRefreshing = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var lista = await _logradouroService.ObterTodosAsync(cts.Token);

            _allLogradouros.Clear();
            _allLogradouros.AddRange(lista);

            FilterLogradouros();
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A conexão com o banco expirou.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private void FilterLogradouros()
    {
        var filtrados = string.IsNullOrWhiteSpace(SearchText)
            ? _allLogradouros
            : SelectedFilterOption switch
            {
                "ID" => _allLogradouros.Where(l => l.Id.ToString().Contains(SearchText.Trim())),
                "CEP" => _allLogradouros.Where(l => l.Cep.Contains(SearchText.Trim())),
                _ => _allLogradouros.Where(l => l.Cidade.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase))
            };

        Logradouros.Clear();
        foreach (var item in filtrados)
        {
            Logradouros.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        await Shell.Current.GoToAsync("logradouro");
    }

    [RelayCommand]
    public async Task EditLogradouroAsync(LogradouroDto? logradouro)
    {
        if (logradouro == null) return;
        await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
    }

    [RelayCommand]
    public async Task DeleteLogradouroAsync(LogradouroDto? logradouro)
    {
        if (logradouro == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmação",
            $"Deseja realmente remover o logradouro {logradouro.Nome}?",
            "Sim",
            "Não");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool removido = await _logradouroService.RemoverAsync(logradouro.Id, cts.Token);

            if (removido)
            {
                _allLogradouros.Remove(logradouro);
                Logradouros.Remove(logradouro);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro removido com sucesso.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Não foi possível remover o logradouro.", "OK");
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Operação Bloqueada",
                    "Este logradouro não pode ser excluído pois está vinculado a alunos ou colaboradores.",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
