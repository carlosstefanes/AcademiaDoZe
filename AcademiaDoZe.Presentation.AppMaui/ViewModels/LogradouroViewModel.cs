using AcademiaDoZe.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroEdicao), "Logradouro")]
public partial class LogradouroViewModel : BaseViewModel, IQueryAttributable
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _cep = string.Empty;

    [ObservableProperty]
    private string _logradouroNome = string.Empty;

    [ObservableProperty]
    private string _bairro = string.Empty;

    [ObservableProperty]
    private string _cidade = string.Empty;

    [ObservableProperty]
    private string _uf = string.Empty;

    private object? _logradouroEdicao;
    public object? LogradouroEdicao
    {
        get => _logradouroEdicao;
        set => SetProperty(ref _logradouroEdicao, value);
    }

    public LogradouroViewModel()
    {
        Title = "Novo Logradouro";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Logradouro", out var logradouroObj))
        {
            LogradouroEdicao = logradouroObj;
            Title = "Editar Logradouro";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Cep) || string.IsNullOrWhiteSpace(LogradouroNome))
        {
            await Shell.Current.DisplayAlert("Validação", "Informe ao menos o CEP e o Logradouro.", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            await Shell.Current.DisplayAlert("Sucesso", "Logradouro salvo com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao salvar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}