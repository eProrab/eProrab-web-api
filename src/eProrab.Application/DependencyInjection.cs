using System.Reflection;
using eProrab.Application.Interfaces;
using eProrab.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eProrab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<ISpecializationService, SpecializationService>();
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ICalculationService, CalculationService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
