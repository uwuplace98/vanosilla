using Coravel.Pro;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PhoenixLib.DAL.Redis;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.Database.DB;
using System;
using WingsAPI.Communication.Caligor;
using WingsAPI.Communication.Compliments;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.InstantBattle;
using WingsAPI.Communication.Icebreaker;
using WingsAPI.Communication.Miniland;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.Quests;
using WingsAPI.Communication.Raid;
using WingsAPI.Communication.RainbowBattle;
using WingsAPI.Communication.TalentArena;
using WingsAPI.Plugins;
using WingsAPI.Plugins.Exceptions;
using WingsEmu.ClusterScheduler.Metrics;
using WingsEmu.ClusterScheduler.Service;
using WingsEmu.ClusterScheduler.Utility;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Game;
using WingsEmu.Health.Extensions;
using WingsEmu.Plugins.DistributedGameEvents;
using WingsEmu.Plugins.DistributedGameEvents.BotMessages;

namespace WingsEmu.ClusterScheduler
{
    public class Startup
    {
        private static ServiceProvider GetPluginsProvider()
        {
            var pluginBuilder = new ServiceCollection();
            pluginBuilder.AddTransient<IDependencyInjectorPlugin, ScheduledEventPublisherCorePlugin>();
            return pluginBuilder.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            new DatabasePlugin().AddDependencies(services);
            using ServiceProvider plugins = GetPluginsProvider();
            foreach (IDependencyInjectorPlugin plugin in plugins.GetServices<IDependencyInjectorPlugin>())
            {
                try
                {
                    Log.Debug($"[PLUGIN_LOADER] Loading generic plugin {plugin.Name}...");
                    plugin.AddDependencies(services);
                }
                catch (PluginException e)
                {
                    Log.Error($"{plugin.Name} : plugin.OnLoad", e);
                }
            }

            services.AddPhoenixLogging();
            // add redis
            services.TryAddSingleton(s => RedisConfiguration.FromEnv());
            services.TryAddSingleton(s => s.GetRequiredService<RedisConfiguration>().GetConnectionMultiplexer());
            services.AddEventPipeline();
            services.AddEventHandlersInAssembly<Startup>();
            services.AddSingleton<IRandomGenerator, RandomGenerator>();

            services.AddMaintenanceMode();
            services.AddMqttConfigurationFromEnv();
            services.AddMessagePublisher<BotMessageMessage>();
            services.AddMessagePublisher<InstantBattleStartMessage>();
            services.AddMessagePublisher<RainbowBattleStartMessage>();
            services.AddMessagePublisher<RainbowBattleLeaverBusterResetMessage>();
            services.AddMessagePublisher<RankingRefreshMessage>();
            services.AddMessagePublisher<CaligorStartMessage>();
            services.AddMessagePublisher<SpecialistPointsRefreshMessage>();
            services.AddMessagePublisher<RaidRestrictionRefreshMessage>();
            services.AddMessagePublisher<QuestDailyRefreshMessage>();
            services.AddMessagePublisher<MinigameRefreshProductionPointsMessage>();
            services.AddMessagePublisher<FamilyMissionsResetMessage>();
            services.AddMessagePublisher<ComplimentsMonthlyRefreshMessage>();
            services.AddMessagePublisher<IcebreakerStartMessage>();
            services.AddMessagePublisher<TalentArenaStartMessage>();

            services.TryAddConnectionMultiplexerFromEnv();

            services.AddGrpcDbServerServiceClient();
            services.AddServerApiServiceClient();
            services.AddGrpcSessionServiceClient();
            services.AddGrpcMailServiceClient();
            services.AddGrpcRelationServiceClient();
            services.AddGrpcClusterStatusServiceClient();
            services.AddClusterCharacterServiceClient();
            services.AddTranslationsGrpcClient();

            /*
             * WARM UP SERVICES
             */
            // Add authentication services
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Coravel/login"; // Set the login page
                    options.AccessDeniedPath = "/Coravel/AccessDenied"; // Set the access denied page
                });

            // Other service configurations.
            //services.AddRazorPages().AddNewtonsoftJson();
            services.AddRazorPages()
                    .AddNewtonsoftJson()
                    .AddRazorPagesOptions(options =>
                    {
                        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                    });

            services.AddScoped<SpecialistPointsRefreshCronScheduler>();
            services.AddScoped<RankingRefreshCronScheduler>();
            services.AddScoped<RainbowBattleLeaverBusterCronScheduler>();
            services.AddScoped<RainbowBattleCronScheduler>();
            services.AddScoped<RaidRestrictionRefreshCronScheduler>();
            services.AddScoped<QuestDailyRefreshCronScheduler>();
            services.AddScoped<MinigameProductionRefreshCronScheduler>();
            services.AddScoped<MinigameProductionRefreshCronScheduler>();
            services.AddScoped<InstantBattleCronScheduler>();
            services.AddScoped<IcebreakerCronScheduler>();
            services.AddScoped<FamilyMissionsResetCronScheduler>();
            services.AddScoped<ComplimentsMonthlyRefreshCronScheduler>();
            services.AddScoped<CaligorCronScheduler>();
            services.AddScoped<TalentArenaCronScheduler>();

            services.AddCoravelPro(typeof(GameContext));
            services.AddPermission<AdminHasPermission>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Enable authentication middleware
            app.UseAuthentication();
            //app.UseAuthorization();

            // Redirect to Coravel login page for 404 errors (Page Not Found)
            app.UseStatusCodePagesWithRedirects("/Coravel/login");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            app.UseCoravelPro();
            app.ClearMetrics();
            app
              .UseMetric<CharacterCountMetric>()
              //.UseMetric<OnlineCountMetric>()
              .UseMetric<UserCountMetric>();
        }
    }
}