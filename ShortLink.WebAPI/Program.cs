using ShortLink.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShortLinkServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseRedirectMiddleware();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();