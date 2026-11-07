using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PhoenixLib.Configuration;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Plugins.AdminRestApi.Configuration;
using WingsEmu.Plugins.AdminRestApi.Middleware;

namespace WingsEmu.Plugins.AdminRestApi
{
    public class AdminRestApiPlugin : IGameServerPlugin
    {
        public string Name => nameof(AdminRestApiPlugin);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            // Load configuration
            services.AddFileConfiguration<AdminApiConfigurationFile>();

            // The plugin will configure Kestrel in GameChannel to expose the API
            // All controllers are already in this assembly and will be picked up by GameChannel's AddControllers()
        }

        public static void ConfigureApp(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<AdminApiConfigurationFile>();

            if (!config.Enabled)
            {
                Log.Info("[ADMIN_REST_API] REST API is disabled in configuration");
                return;
            }

            // Configure CORS
            if (config.AllowCors)
            {
                app.UseCors(builder =>
                {
                    if (config.CorsOrigins != null && Array.Exists(config.CorsOrigins, x => x == "*"))
                    {
                        builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                    else
                    {
                        builder.WithOrigins(config.CorsOrigins ?? new[] { "*" })
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            }

            // Add API key authentication middleware
            app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

            Log.Info("[ADMIN_REST_API] REST API enabled on GameChannel");
            if (config.EnableSwagger)
            {
                Log.Info("[ADMIN_REST_API] Swagger UI available at: http://localhost:{port}/swagger");
            }
            Log.Info($"[ADMIN_REST_API] API Key: {config.ApiKey}");
            Log.Info($"[ADMIN_REST_API] CORS Enabled: {config.AllowCors}");
        }

        public static void ConfigureSwagger(IServiceCollection services, AdminApiConfigurationFile config)
        {
            if (!config.Enabled || !config.EnableSwagger)
            {
                return;
            }

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WingsEmu Admin REST API",
                    Version = "v1",
                    Description = "REST API for WingsEmu NosTale Private Server Administration"
                });

                // Resolve conflicting actions by taking the first one
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key authentication using X-API-Key header",
                    Name = "X-API-Key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKeyScheme"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}
