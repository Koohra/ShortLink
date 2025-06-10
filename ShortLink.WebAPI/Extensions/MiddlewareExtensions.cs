using ShortLink.WebAPI.Middleware;

namespace ShortLink.WebAPI.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseRedirectMiddleware(this WebApplication app)
    {
        app.UseMiddleware<RedirectMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}