using Microsoft.Extensions.Options;
using ShortLink.Application.Commands.CreateLink;
using ShortLink.Application.Common;

namespace ShortLink.WebAPI.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar opções da aplicação
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Registrar MediatR (.NET 10 syntax)
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(CreateLinkHandler).Assembly); });

        return services;
    }
}