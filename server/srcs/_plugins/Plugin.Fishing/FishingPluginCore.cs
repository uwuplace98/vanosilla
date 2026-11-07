using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.Fishing.Configuration;
using Plugin.ResourceLoader.Loaders;
using WingsEmu.Game._ItemUsage;
using Plugin.Fishing.Event;
using WingsAPI.Plugins;
using PhoenixLib.Configuration;
using WingsEmu.Game.Core.SkillHandling;
using Plugin.Fishing.Handlers;
using WingsEmu.Game.Characters.Events;
using PhoenixLib.Events;
using System;

namespace Plugin.Fishing
{
    public class FishingPluginCore : IGameServerPlugin, IGamePlugin
    {
        private readonly ISkillHandlerContainer _skillHandlerContainer;
        private readonly IServiceProvider _serviceProvider;

        public FishingPluginCore(ISkillHandlerContainer skillHandlerContainer, IServiceProvider serviceProvider)
        {
            _skillHandlerContainer = skillHandlerContainer;
            _serviceProvider = serviceProvider;
        }

        public string Name => nameof(FishingPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            // Load base configuration from YAML (can be used for overrides)
            services.AddFileConfiguration<FishingConfiguration>("fishing_configuration");

            // Use the ResourceLoader to load fish.dat data
            services.AddSingleton(sp =>
            {
                var yamlConfig = sp.GetRequiredService<FishingConfiguration>();
                var resourceLoader = sp.GetRequiredService<FishingResourceFileLoader>();
                
                // Load fishing data from fish.dat
                var spotsTask = resourceLoader.LoadAsync();
                spotsTask.Wait();
                var spots = spotsTask.Result;
                var drops = resourceLoader.Drops;

                // Populate configuration from loaded data
                foreach (var spot in spots)
                {
                    // Check if map already exists
                    if (!yamlConfig.Maps.Exists(m => m.MapVnum == spot.MapId))
                    {
                        yamlConfig.Maps.Add(new FishingMap
                        {
                            MapVnum = spot.MapId,
                            RegionId = spot.RegionId
                        });
                    }

                    yamlConfig.Pools.Add(new FishingPool
                    {
                        RegionId = spot.RegionId,
                        X = spot.X,
                        Y = spot.Y,
                        Direction = spot.Direction
                    });
                }

                foreach (var drop in drops)
                {
                    yamlConfig.Drops.Add(new FishDrop
                    {
                        Id = drop.Id,
                        Vnum = drop.ItemVnum,
                        Chance = drop.Chance
                    });
                }

                return yamlConfig;
            });

            services.TryAddSingleton<IFishingManager, FishingManager>();
            services.AddTransient<IItemUsageByVnumHandler, FishingItemUsageHandler>();
            services.AddTransient<FishingSkillHandler>();
            services.AddTransient<IAsyncEventProcessor<CharacterLoadEvent>, FishingCharacterLoadEventHandler>();
        }

        public void OnLoad()
        {
            var handler = _serviceProvider.GetService<FishingSkillHandler>();
            if (handler != null)
            {
                _skillHandlerContainer.Register(handler);
            }
        }
    }
}
