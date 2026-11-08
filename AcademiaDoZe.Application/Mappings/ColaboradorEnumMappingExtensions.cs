using System;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorEnumMappingExtensions
{
    public static ColaboradorTipo ToDomain(this AppColaboradorTipo tipo) => tipo switch
    {
        AppColaboradorTipo.Administrador => ColaboradorTipo.Administrador,
        AppColaboradorTipo.Atendente => ColaboradorTipo.Atendente,
        AppColaboradorTipo.Instrutor => ColaboradorTipo.Instrutor,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
    };

    public static AppColaboradorTipo ToApplication(this ColaboradorTipo tipo) => tipo switch
    {
        ColaboradorTipo.Administrador => AppColaboradorTipo.Administrador,
        ColaboradorTipo.Atendente => AppColaboradorTipo.Atendente,
        ColaboradorTipo.Instrutor => AppColaboradorTipo.Instrutor,
        ColaboradorTipo.Recepcionista => AppColaboradorTipo.Atendente,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
    };

    public static ColaboradorVinculo ToDomain(this AppColaboradorVinculo vinculo) => vinculo switch
    {
        AppColaboradorVinculo.CLT => ColaboradorVinculo.CLT,
        AppColaboradorVinculo.Estagio => ColaboradorVinculo.Estagio,
        _ => throw new ArgumentOutOfRangeException(nameof(vinculo), vinculo, null)
    };

    public static AppColaboradorVinculo ToApplication(this ColaboradorVinculo vinculo) => vinculo switch
    {
        ColaboradorVinculo.CLT => AppColaboradorVinculo.CLT,
        ColaboradorVinculo.Estagio => AppColaboradorVinculo.Estagio,
        ColaboradorVinculo.PJ => AppColaboradorVinculo.CLT,
        _ => throw new ArgumentOutOfRangeException(nameof(vinculo), vinculo, null)
    };
}

