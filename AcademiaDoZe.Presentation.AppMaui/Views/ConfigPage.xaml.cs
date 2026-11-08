using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }

    private void CarregarTema()
    {
        string tema = Preferences.Get("Tema", "system");
        TemaPicker.SelectedItem = tema;
    }

    private async void OnSalvarTemaClicked(object? sender, EventArgs e)
    {
        if (TemaPicker.SelectedItem is string tema)
        {
            Preferences.Set("Tema", tema);
            WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
            await DisplayAlertAsync("Sucesso", "Tema salvo com sucesso!", "OK");
        }
    }

    private void CarregarBanco()
    {
        DatabaseTypePicker.ItemsSource = Enum.GetValues<AppDatabaseType>();
        string tipoBancoSalvo = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());

        if (Enum.TryParse<AppDatabaseType>(tipoBancoSalvo, out var dbType))
        {
            DatabaseTypePicker.SelectedItem = dbType;
        }
        else
        {
            DatabaseTypePicker.SelectedItem = AppDatabaseType.Sqlite;
        }

        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(object? sender, EventArgs e)
    {
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not AppDatabaseType tipo)
            return;

        string prefix = tipo.ToString();
        bool isSqlite = tipo == AppDatabaseType.Sqlite;

        SqliteInfoCard.IsVisible = isSqlite;
        SqliteContainer.IsVisible = isSqlite;
        ServidorBancoGrid.IsVisible = !isSqlite;
        CredenciaisGrid.IsVisible = !isSqlite;
        ComplementoLabel.Text = isSqlite ? "Parâmetros Adicionais (opcional)" : "Complemento";

        if (isSqlite)
        {
            SqliteCaminhoEntry.Text = Preferences.Get($"{prefix}_Caminho", Preferences.Get("Database_Caminho", @"D:\atv 5\academia_test.db"));
            ComplementoEntry.Text = Preferences.Get($"{prefix}_Complemento", "");
        }
        else
        {
            ServidorEntry.Text = Preferences.Get($"{prefix}_Servidor", Preferences.Get("Database_Servidor", "localhost"));
            BancoEntry.Text = Preferences.Get($"{prefix}_Banco", Preferences.Get("Database_Banco", "AcademiaDoZe"));
            UsuarioEntry.Text = Preferences.Get($"{prefix}_Usuario", Preferences.Get("Database_Usuario", "root"));
            SenhaEntry.Text = Preferences.Get($"{prefix}_Senha", Preferences.Get("Database_Senha", ""));
            ComplementoEntry.Text = Preferences.Get($"{prefix}_Complemento", "");
        }
    }

    private async void OnSalvarBdClicked(object? sender, EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not AppDatabaseType tipo)
        {
            await DisplayAlertAsync("Erro", "Selecione o tipo de gerenciador.", "OK");
            return;
        }

        string prefix = tipo.ToString();
        Preferences.Set("DatabaseType", tipo.ToString());

        if (tipo == AppDatabaseType.Sqlite)
        {
            string caminho = SqliteCaminhoEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(caminho))
            {
                await DisplayAlertAsync("Atenção", "Informe o caminho do banco SQLite.", "OK");
                return;
            }

            string complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;
            Preferences.Set($"{prefix}_Caminho", caminho);
            Preferences.Set($"{prefix}_Complemento", complemento);

            // Chaves compatíveis
            Preferences.Set("Database_Caminho", caminho);
            Preferences.Set("Database_Complemento", complemento);
        }
        else
        {
            string servidor = ServidorEntry.Text?.Trim() ?? string.Empty;
            string banco = BancoEntry.Text?.Trim() ?? string.Empty;
            string usuario = UsuarioEntry.Text?.Trim() ?? string.Empty;
            string senha = SenhaEntry.Text ?? string.Empty;
            string complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(banco) || string.IsNullOrWhiteSpace(usuario))
            {
                await DisplayAlertAsync("Atenção", "Preencha Servidor, Banco e Usuário.", "OK");
                return;
            }

            Preferences.Set($"{prefix}_Servidor", servidor);
            Preferences.Set($"{prefix}_Banco", banco);
            Preferences.Set($"{prefix}_Usuario", usuario);
            Preferences.Set($"{prefix}_Senha", senha);
            Preferences.Set($"{prefix}_Complemento", complemento);

            // Chaves compatíveis
            Preferences.Set("Database_Servidor", servidor);
            Preferences.Set("Database_Banco", banco);
            Preferences.Set("Database_Usuario", usuario);
            Preferences.Set("Database_Senha", senha);
            Preferences.Set("Database_Complemento", complemento);
        }

        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));
        await DisplayAlertAsync("Sucesso", "Configurações de banco salvas com sucesso!", "OK");
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}

