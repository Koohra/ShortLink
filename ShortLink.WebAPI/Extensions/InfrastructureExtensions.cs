using ShortLink.Domain.Interfaces;
using ShortLink.Infrastructure.Cache;
using ShortLink.Infrastructure.ExternalServices;
using ShortLink.Infrastructure.Repositories;

namespace ShortLink.WebAPI.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Serviços de infraestrutura
        services.AddScoped<IShortCodeGenerator, ShortCodeGenerator>();
        
        return services;
    }
}