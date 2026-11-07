using PhoenixLib.Events;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Revival;

namespace Plugin.TalentArena.EventHandlers;

public class RevivalStartProcedureTalentArenaEventHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISessionManager _sessionManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public RevivalStartProcedureTalentArenaEventHandler(IAsyncEventPipeline asyncEventPipeline, ISessionManager sessionManager, IGameLanguageService gameLanguage)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || !e.Sender.PlayerEntity.TalentArenaComponent.IsInTalentArena || e.Sender.PlayerEntity.TalentArenaComponent.Instance == null)
        {
            return;
        }

        ITalentArenaComponent senderArenaComponent = e.Sender.PlayerEntity.TalentArenaComponent;
        ArenaTeamType teamType = senderArenaComponent.TeamType;

        senderArenaComponent.IsInFightZone = false;

        if (!e.Sender.PlayerEntity.TalentArenaComponent.IsSummoned)
        {
            senderArenaComponent.IsDead = true;
            switch (teamType)
            {
                case ArenaTeamType.ZENAS:
                    senderArenaComponent.Instance.EreniaTeam.Score++;
                    break;

                case ArenaTeamType.ERENIA:
                    senderArenaComponent.Instance.ZenasTeam.Score++;
                    break;
            }
        }

        if (e.Sender.PlayerEntity.TalentArenaComponent.IsSummoned)
        {
            e.Sender.PlayerEntity.TalentArenaComponent.IsSummoned = false;
        }
        else
        {
            await _asyncEventPipeline.ProcessEventAsync(new TalentArenaFinishRoundEvent(senderArenaComponent.Instance), cancellation);
        }

        var pos = new Position(senderArenaComponent.TeamType == ArenaTeamType.ZENAS ? (short)125 : (short)14, 38);
        e.Sender.PlayerEntity.TeleportOnMap(pos.X, pos.Y);
        e.Sender.SendPacket(TalentArenaExtensions.GenerateTaSt(TalentArenaOptionType.Watch));

        e.Sender.PlayerEntity.UpdateRevival(DateTime.UtcNow + TimeSpan.FromSeconds(4), RevivalType.DontPayRevival, ForcedType.Reconnect);
    }
}