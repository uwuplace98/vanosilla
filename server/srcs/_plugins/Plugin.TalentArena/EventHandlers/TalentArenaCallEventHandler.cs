using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaCallEventHandler : IAsyncEventProcessor<TalentArenaCallEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IScheduler _scheduler;

    private const byte TIMER = 30;

    public TalentArenaCallEventHandler(IAsyncEventPipeline asyncEventPipeline, IScheduler scheduler)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _scheduler = scheduler;
    }

    public async Task HandleAsync(TalentArenaCallEvent e, CancellationToken notification)
    {
        TalentArenaTeam ownTeam = e.Team;
        ITalentArenaComponent senderArenaComponent = e.Sender.PlayerEntity.TalentArenaComponent;
        var newPos = new Position(e.Sender.PlayerEntity.PositionX, e.Sender.PlayerEntity.PositionY);

        ownTeam.Calls++;
        ownTeam.Members.ForEach(arenaUser => { arenaUser.SendPacket(arenaUser.GenerateTaP(2, true)); });

        e.Sender.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.GenerateEffectPacket(4432));

        e.Client.PlayerEntity.TalentArenaComponent.LastSummon = DateTime.UtcNow;
        _scheduler.Schedule(TimeSpan.FromSeconds(3), async () =>
        {
            e.Sender.CurrentMapInstance.Broadcast($"ta_t 0 {e.Client.PlayerEntity.Id} {TIMER}");

            await _asyncEventPipeline.ProcessEventAsync(new TalentArenaTeleportToFightZoneEvent(senderArenaComponent.Instance, true)
            {
                Position = newPos,
                MemberCalled = e.Client
            }, notification);
        });

        _scheduler.Schedule(TimeSpan.FromSeconds(TIMER + 3), async () =>
        {
            if (!e.Client.PlayerEntity.TalentArenaComponent.IsSummoned || e.Client.PlayerEntity.TalentArenaComponent.LastSummon.AddSeconds(TIMER) >= DateTime.UtcNow)
            {
                return;
            }

            e.Client.PlayerEntity.Hp = e.Client.PlayerEntity.MaxHp;
            e.Client.PlayerEntity.Mp = e.Client.PlayerEntity.MaxMp;

            await e.Client.PlayerEntity.RemoveNegativeBuffs(100);

            var pos = new Position(senderArenaComponent.TeamType == ArenaTeamType.ZENAS ? (short)125 : (short)14, 38);
            e.Client.PlayerEntity.TeleportOnMap(pos.X, pos.Y);
            e.Client.SendPacket(TalentArenaExtensions.GenerateTaSt(TalentArenaOptionType.Watch));
            e.Client.PlayerEntity.TalentArenaComponent.IsSummoned = false;
        });
    }
}