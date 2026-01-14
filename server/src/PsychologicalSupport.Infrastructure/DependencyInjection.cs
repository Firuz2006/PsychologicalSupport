using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Infrastructure.Data;
using PsychologicalSupport.Infrastructure.ExternalServices;
using PsychologicalSupport.Infrastructure.Repositories;
using PsychologicalSupport.Infrastructure.Services;

namespace PsychologicalSupport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ILlmMatchingService, LlmMatchingService>();

        return services;
    }
}
