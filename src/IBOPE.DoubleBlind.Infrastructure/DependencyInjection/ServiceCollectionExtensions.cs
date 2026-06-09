using IBOPE.DoubleBlind.Application.Interfaces;
using IBOPE.DoubleBlind.Application.Options;
using IBOPE.DoubleBlind.Infrastructure.Repositories;
using IBOPE.DoubleBlind.Infrastructure.Security;
using IBOPE.DoubleBlind.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IBOPE.DoubleBlind.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<ITaskExecutionService, TaskExecutionService>();
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddScoped<ILoginService, AuthenticationService>();

        return services;
    }
}
