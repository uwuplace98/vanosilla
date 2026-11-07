using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.TalentArena.Managers;
using Plugin.TalentArena.RecurrentJob;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.TalentArena;
using WingsAPI.Plugins;
using WingsEmu.Game.Arena.TalentArena;

namespace Plugin.TalentArena;

public class TalentArenaPluginCore : IGameServerPlugin
{
    public string Name => nameof(TalentArenaPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddEventHandlersInAssembly<TalentArenaPlugin>();
        services.AddSingleton<ITalentArenaManager, TalentArenaManager>();
        services.AddMessagePublisher<TalentArenaStartMessage>();
        services.AddMessageSubscriber<TalentArenaStartMessage, TalentArenaStartMessageConsumer>();
        services.AddSingleton<ITalentArenaFactory, TalentArenaFactory>();

        if (gameServer.Type == GameChannelType.ACT_4)
        {
            return;
        }

        services.AddHostedService<TalentArenaSystem>();
    }
}