using PhoenixLib.Events;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaFinishRoundEventHandler : IAsyncEventProcessor<TalentArenaFinishRoundEvent>
{
    private readonly ITalentArenaManager _talentArenaManager;

    public TalentArenaFinishRoundEventHandler(ITalentArenaManager talentArenaManager) => _talentArenaManager = talentArenaManager;

    public async Task HandleAsync(TalentArenaFinishRoundEvent e, CancellationToken notification)
    {
        TalentArenaInstance instance = e.Instance;

        if (!instance.IsFighting)
        {
            return;
        }

        // Include members called that are in fight zone
        var membersInFight = new List<IClientSession>();
        membersInFight.AddRange(instance.MapInstance.Sessions.Where(s => s.PlayerEntity.TalentArenaComponent.IsSummoned));
        foreach (IClientSession member in membersInFight)
        {
            ITalentArenaComponent talentArenaComponent = member.PlayerEntity.TalentArenaComponent;
            member.CurrentMapInstance.Broadcast($"ta_t 0 {member.PlayerEntity.Id} 0");
            talentArenaComponent.IsSummoned = false;

            await TeleportToLobby(member);
        }

        instance.IsFighting = false;
        instance.EventDateTick = DateTime.UtcNow;
        instance.SecondsAmount = TimeSpan.FromSeconds(5);
    }

    private async Task TeleportToLobby(IClientSession member)
    {
        member.PlayerEntity.Hp = member.PlayerEntity.MaxHp;
        member.PlayerEntity.Mp = member.PlayerEntity.MaxMp;

        await member.PlayerEntity.RemoveNegativeBuffs(100);

        var pos = new Position(member.PlayerEntity.TalentArenaComponent.TeamType == ArenaTeamType.ZENAS ? (short)125 : (short)14, 38);
        member.PlayerEntity.TeleportOnMap(pos.X, pos.Y);
        member.SendPacket(TalentArenaExtensions.GenerateTaSt(TalentArenaOptionType.Watch));
    }
}