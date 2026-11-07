using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.LandOfDeath.Command;
using Plugin.LandOfDeath.Managers;
using Plugin.LandOfDeath.RecurrentJob;
using WingsAPI.Communication.LandOfDeath;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.LandOfDeath;

namespace Plugin.LandOfDeath;

public class LandOfDeathPluginCore : IGameServerPlugin
{
    public string Name => nameof(LandOfDeathPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddEventHandlersInAssembly<LandOfDeathPlugin>();
        services.AddSingleton<ILandOfDeathManager, LandOfDeathManager>();
        services.AddMessagePublisher<LandOfDeathStartMessage>();
        services.AddMessageSubscriber<LandOfDeathStartMessage, LandOfDeathStartMessageConsumer>();
        services.AddSingleton<ILandOfDeathFactory, LandOfDeathFactory>();
        if (gameServer.Type == GameChannelType.ACT_4)
        {
            return;
        }

        services.AddHostedService<LandOfDeathSystem>();
    }
}

public class LandOfDeathPlugin : IGamePlugin
{
    private readonly ICommandContainer _commandContainer;

    public LandOfDeathPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

    public string Name => nameof(LandOfDeathPlugin);

    public void OnLoad()
    {
        _commandContainer.AddModule<LandOfDeathCommandModule>();
    }
}