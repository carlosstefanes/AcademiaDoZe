using System;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    private readonly LogradouroListViewModel _viewModel;

    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadLogradourosCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is LogradouroDto logradouro)
        {
            await _viewModel.EditLogradouroCommand.ExecuteAsync(logradouro);
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is LogradouroDto logradouro)
        {
            await _viewModel.DeleteLogradouroCommand.ExecuteAsync(logradouro);
        }
    }
}
