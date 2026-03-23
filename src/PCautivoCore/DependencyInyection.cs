using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Infrastructure.Persistence;
using PCautivoCore.Infrastructure.Persistence.Repositories;
using PCautivoCore.Infrastructure.Repositories;
using PCautivoCore.Infrastructure.Services;
using PCautivoCore.Infrastructure.Settings;
using PCautivoCore.Shared.Behaviors;
using PCautivoCore.Shared.Requests;
using PCautivoCore.Shared.Settings;
using PCautivoCore.Shared.Utils;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Reflection;

namespace PCautivoCore;

public static class DependencyInyection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthContext, AuthContextService>();
        services.AddScoped<IJwtUtil, JwtUtil>();


        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCache();

        services.AddSingleton<MssqlContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddScoped<IUserDetailRepository, UserDetailRepository>();
        services.AddScoped<IUserPropertyRepository, UserPropertyRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IDeviceSessionRepository, DeviceSessionRepository>();

        // Servicios de notificaciones
        services.AddSingleton<Infrastructure.Queue.AzureQueueClient>();

        // Configuraciones
        services.Configure<Infrastructure.Queue.AzureQueueSettings>(configuration.GetSection("Settings:AzureQueue"));
        services.Configure<Infrastructure.Queue.AzureQueueCredentials>(configuration.GetSection("Credentials:AzureQueue"));
        services.Configure<Infrastructure.Settings.SystemSettings>(configuration.GetSection("Settings:System"));
        services.Configure<Infrastructure.Settings.NotificationSettings>(configuration.GetSection("Settings:Notifications"));
        services.Configure<OmadaSyncJobSettings>(configuration.GetSection("OmadaSyncJob"));

        services.AddRefit(configuration);

        // Omada OC-300 (Portal Cautivo)
        services.Configure<OmadaSettings>(configuration.GetSection("Omada"));
        services.AddSingleton<IOmadaService, OmadaService>();

        return services;
    }

    private static IServiceCollection AddRefit(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Providers:Hostaway");




        return services;
    }


    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }
}
