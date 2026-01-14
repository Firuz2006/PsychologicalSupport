using Microsoft.Extensions.DependencyInjection;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Application.Services;

namespace PsychologicalSupport.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPsychologistService, PsychologistService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IMatchingService, MatchingService>();
        services.AddHttpClient();

        return services;
    }
}
