using System;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ILogradouroService, LogradouroService>();
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IMatriculaService, MatriculaService>();

        services.AddTransient<Func<ILogradouroRepository>>(sp => () =>
        {
            var config = sp.GetRequiredService<RepositoryConfig>();
            return new LogradouroRepository(config.ConnectionString, config.DatabaseType.ToInfrastructure());
        });

        services.AddTransient<Func<IAlunoRepository>>(sp => () =>
        {
            var config = sp.GetRequiredService<RepositoryConfig>();
            return new AlunoRepository(config.ConnectionString, config.DatabaseType.ToInfrastructure());
        });

        services.AddTransient<Func<IColaboradorRepository>>(sp => () =>
        {
            var config = sp.GetRequiredService<RepositoryConfig>();
            return new ColaboradorRepository(config.ConnectionString, config.DatabaseType.ToInfrastructure());
        });

        services.AddTransient<Func<IMatriculaRepository>>(sp => () =>
        {
            var config = sp.GetRequiredService<RepositoryConfig>();
            return new MatriculaRepository(config.ConnectionString, config.DatabaseType.ToInfrastructure());
        });

        return services;
    }
}

