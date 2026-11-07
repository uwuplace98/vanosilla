using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Minigames.Configuration;
using Plugin.Minigames.Event.Guri;
using Plugin.Minigames.ItemUsage;
using Plugin.Minigames.Managers;
using WingsAPI.Plugins;
using WingsEmu.Game._Guri;
using WingsEmu.Game.Inventory;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage;

namespace Plugin.Minigames
{
    public class MinigamesPluginCore : IGameServerPlugin
    {
        public string Name => nameof(MinigamesPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            // Configuration
            services.AddEventHandlersInAssembly<MinigamesPlugin>();
            services.AddFileConfiguration<MinigameConfiguration>("minigame_configuration");

            // Managers
            services.TryAddSingleton<ISheepFarmManager, SheepFarmManager>();
            services.TryAddSingleton<IMeteoriteGameManager, MeteoriteGameManager>();

            // Guri Handler - Unified handler for all minigame joins (guri 506)
            services.AddTransient<IGuriHandler, MinigameEventGuriHandler>();

            // Item Usage Handlers
            services.AddTransient<IItemUsageByVnumHandler, MinigameItemUsageHandler>();
        }
    }
}
