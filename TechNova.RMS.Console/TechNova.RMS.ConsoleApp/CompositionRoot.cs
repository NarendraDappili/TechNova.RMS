using Microsoft.Extensions.DependencyInjection;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;
using TechNova.RMS.ConsoleApp.Infrastructure.Json;
using TechNova.RMS.ConsoleApp.Infrastructure.Notifications;

namespace TechNova.RMS.ConsoleApp;

internal static class CompositionRoot
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Repositories
        services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<IAllocationRepository, AllocationRepository>();

        // Application services
        services.AddSingleton<INotificationService, ConsoleNotificationService>();
        services.AddSingleton<IAllocationService, AllocationService>();
        services.AddSingleton<IEmployeeService, EmployeeService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IMatcher, SimpleMatcher>();

        // Console app
        services.AddSingleton<ConsoleMenu>();

        // Seed data on startup via service provider later
    }
}
