using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (BindingContext is AcessoViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AcessoPage OnAppearing] Erro: {ex}");
            await DisplayAlertAsync("Erro", $"Erro ao carregar Controle de Acesso: {ex.Message}", "OK");
        }
    }
}
