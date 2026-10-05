using System.Collections.ObjectModel;
using AcademiaDoZe.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<object> Logradouros { get; } = new();

    public LogradouroListViewModel()
    {
        Title = "Logradouros";
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsRefreshing = true;

            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        await Shell.Current.GoToAsync("LogradouroPage");
    }

    [RelayCommand]
    private async Task EditLogradouroAsync(object logradouro)
    {
        if (logradouro == null) return;

        var navigationParams = new Dictionary<string, object>
        {
            { "Logradouro", logradouro }
        };

        await Shell.Current.GoToAsync("LogradouroPage", navigationParams);
    }
}