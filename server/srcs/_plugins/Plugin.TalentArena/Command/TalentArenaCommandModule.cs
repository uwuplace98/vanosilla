using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication.TalentArena;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Extensions;

namespace Plugin.TalentArena.Command;

[Name("Talent Arena")]
[Group("aot", "talentarena")]
[RequireAuthority(AuthorityType.GameMaster)]
public class TalentArenaCommandModule : SaltyModuleBase
{
    private readonly IMessagePublisher<TalentArenaStartMessage> _messagePublisher;
    private readonly IAsyncEventPipeline _eventPipeline;

    public TalentArenaCommandModule(IMessagePublisher<TalentArenaStartMessage> messagePublisher, IAsyncEventPipeline eventPipeline)
    {
        _messagePublisher = messagePublisher;
        _eventPipeline = eventPipeline;
    }

    [Command("start")]
    public async Task<SaltyCommandResult> Start()
    {
        Context.Player.SendSuccessChatMessage("Talent arena started");

        await _messagePublisher.PublishAsync(new TalentArenaStartMessage());

        await _eventPipeline.ProcessEventAsync(new TalentArenaStartEvent());

        return new SaltyCommandResult(true);
    }
}