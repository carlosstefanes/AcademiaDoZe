using AcademiaDoZe.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _totalLogradouros;

    [ObservableProperty]
    private int _totalAlunos;

    [ObservableProperty]
    private int _totalColaboradores;

    [ObservableProperty]
    private int _totalMatriculas;

    public DashboardListViewModel()
    {
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Simulação/Leitura inicial de totais para o painel
            await Task.Delay(300);

            TotalAlunos = 0;
            TotalColaboradores = 0;
            TotalMatriculas = 0;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao carregar dados do Dashboard: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLogradourosAsync()
    {
        await Shell.Current.GoToAsync("///LogradouroListPage");
    }
}