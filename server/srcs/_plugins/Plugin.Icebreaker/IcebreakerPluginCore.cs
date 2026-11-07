using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Icebreaker.Event.Icebreaker;
using Plugin.Icebreaker.Event.Icebreaker.Guri;
using WingsAPI.Plugins;
using WingsEmu.Game._Guri;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Managers; // IIcebreakerManager
using Plugin.Icebreaker.Configuration;
using LocalIcebreakerConfig = Plugin.Icebreaker.Configuration.IcebreakerConfiguration;

namespace Plugin.Icebreaker
{
    public class IcebreakerPluginCore : IGameServerPlugin
    {
        public string Name => nameof(IcebreakerPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddEventHandlersInAssembly<IcebreakerPlugin>();
            services.TryAddSingleton<IIcebreakerManager, IcebreakerManager>();
            services.AddTransient<IGuriHandler, IcebreakerJoinEventHandler>();
            services.AddTransient<IGuriHandler, IcebreakerUnfreezeEventHandler>();
            services.AddHostedService<IcebreakerSystem>();
            services.AddFileConfiguration<LocalIcebreakerConfig>("icebreaker_configuration");
        }
    }
}
