using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registo da rota de detalhe/formulário
        Routing.RegisterRoute(nameof(LogradouroPage), typeof(LogradouroPage));
    }
}