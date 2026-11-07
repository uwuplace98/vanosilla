using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication.LandOfDeath;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.LandOfDeath;

namespace Plugin.LandOfDeath.Command;

[Name("Land Of Death")]
[Group("landofdeath", "lod")]
[RequireAuthority(AuthorityType.Owner)]
public class LandOfDeathCommandModule : SaltyModuleBase
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IMessagePublisher<LandOfDeathStartMessage> _messagePublisher;
    private readonly ILandOfDeathManager _landOfDeathManager;

    public LandOfDeathCommandModule(IMessagePublisher<LandOfDeathStartMessage> messagePublisher, ILandOfDeathManager landOfDeathManager)
    {
        _messagePublisher = messagePublisher;
        _landOfDeathManager = landOfDeathManager;
    }

    [Command("start")]
    public async Task<SaltyCommandResult> Start()
    {
        await _messagePublisher.PublishAsync(new LandOfDeathStartMessage(), CancellationToken.None);
        _landOfDeathManager.LandOfDeathProcessTime = DateTime.UtcNow;
        return new SaltyCommandResult(true);
    }
}