using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WingsEmu.Plugins.AdminRestApi.Configuration;

namespace WingsEmu.Plugins.AdminRestApi.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AdminApiConfigurationFile _config;
        private const string API_KEY_HEADER = "X-API-Key";

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, AdminApiConfigurationFile config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger endpoints
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/api/health"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "API Key missing. Provide 'X-API-Key' header." });
                return;
            }

            if (!_config.ApiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
                return;
            }

            await _next(context);
        }
    }
}
