using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    [ObservableProperty]
    private int _logradouroId;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _cep = string.Empty;

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _bairro = string.Empty;

    [ObservableProperty]
    private string _cidade = string.Empty;

    [ObservableProperty]
    private string _estado = string.Empty;

    [ObservableProperty]
    private string _pais = "Brasil";

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Novo Logradouro";
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId > 0)
        {
            Title = "Editar Logradouro";
            try
            {
                IsBusy = true;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var logradouro = await _logradouroService.ObterPorIdAsync(LogradouroId, cts.Token);
                if (logradouro != null)
                {
                    Id = logradouro.Id;
                    Cep = logradouro.Cep;
                    Nome = logradouro.Nome;
                    Bairro = logradouro.Bairro;
                    Cidade = logradouro.Cidade;
                    Estado = logradouro.Estado;
                    Pais = logradouro.Pais;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouro: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        else
        {
            Title = "Novo Logradouro";
            Id = 0;
            Cep = string.Empty;
            Nome = string.Empty;
            Bairro = string.Empty;
            Cidade = string.Empty;
            Estado = string.Empty;
            Pais = "Brasil";
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        if (string.IsNullOrWhiteSpace(Cep)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouro = await _logradouroService.ObterPorCepAsync(Cep.Trim(), cts.Token);
            if (logradouro != null)
            {
                Nome = logradouro.Nome;
                Bairro = logradouro.Bairro;
                Cidade = logradouro.Cidade;
                Estado = logradouro.Estado;
                Pais = logradouro.Pais;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Aviso", $"Não foi possível buscar pelo CEP: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (!ValidateLogradouro(out var errors))
        {
            await Shell.Current.DisplayAlertAsync("Validação", string.Join("\n", errors), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var dto = new LogradouroDto
            {
                Id = Id,
                Cep = Cep.Trim(),
                Nome = Nome.Trim(),
                Bairro = Bairro.Trim(),
                Cidade = Cidade.Trim(),
                Estado = Estado.Trim().ToUpperInvariant(),
                Pais = string.IsNullOrWhiteSpace(Pais) ? "Brasil" : Pais.Trim()
            };

            if (Id > 0)
            {
                await _logradouroService.AtualizarAsync(dto, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro atualizado com sucesso.", "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(dto, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro cadastrado com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateLogradouro(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Cep))
            errors.Add("O CEP é obrigatório.");
        else if (new string(Cep.Where(char.IsDigit).ToArray()).Length != 8)
            errors.Add("O CEP deve conter 8 dígitos.");

        if (string.IsNullOrWhiteSpace(Nome))
            errors.Add("O logradouro/rua é obrigatório.");

        if (string.IsNullOrWhiteSpace(Bairro))
            errors.Add("O bairro é obrigatório.");

        if (string.IsNullOrWhiteSpace(Cidade))
            errors.Add("A cidade é obrigatória.");

        if (string.IsNullOrWhiteSpace(Estado))
            errors.Add("O estado (UF) é obrigatório.");
        else if (Estado.Trim().Length != 2)
            errors.Add("O estado (UF) deve conter 2 letras.");

        return errors.Count == 0;
    }
}
