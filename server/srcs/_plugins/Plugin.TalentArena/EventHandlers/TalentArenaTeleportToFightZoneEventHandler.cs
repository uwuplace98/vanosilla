using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaTeleportToFightZoneEventHandler : IAsyncEventProcessor<TalentArenaTeleportToFightZoneEvent>
{
    private readonly ITalentArenaManager _talentArenaManager;

    public TalentArenaTeleportToFightZoneEventHandler(ITalentArenaManager talentArenaManager) => _talentArenaManager = talentArenaManager;

    public async Task HandleAsync(TalentArenaTeleportToFightZoneEvent e, CancellationToken cancellation)
    {
        TalentArenaInstance instance = e.ArenaInstance;

        IEnumerable<IClientSession> members = e.MemberCalled == null ? _talentArenaManager.GetTeamMembers(instance) : new[] { e.MemberCalled };

        if (!e.IsCall)
        {
            if (instance.Round == 1)
            {
                instance.MapInstance.MapDesignObjects.ToList().ForEach(md => instance.MapInstance.Broadcast(md.GenerateEffectFromDesign(true)));
                instance.MapInstance.MapDesignObjects.Clear();

                foreach (IClientSession member in instance.MapInstance.Sessions)
                {
                    ITalentArenaComponent talentArenaComponent = member.PlayerEntity.TalentArenaComponent;
                    TalentArenaTeam team = talentArenaComponent.TeamType == ArenaTeamType.ZENAS ? instance.ZenasTeam : instance.EreniaTeam;
                    if (talentArenaComponent.Order == -1)
                    {
                        for (byte x = 0; x < 2; x++)
                        {
                            if (team.Members.All(at => at.PlayerEntity.TalentArenaComponent.Order != x))
                            {
                                talentArenaComponent.Order = x;
                            }
                        }
                    }

                    member.SendPacket($"ta_pn {member.PlayerEntity.TalentArenaComponent.Order + 1}");
                }
            }

            foreach (IClientSession member in instance.MapInstance.Sessions)
            {
                member.SendPacket(member.GenerateTaM(2));
                member.SendPacket(member.GenerateTaM(3, (short?)instance.SecondsAmount.TotalSeconds));
            }
        }

        Position newPos = e.Position;
        if (!e.IsCall)
        {
            foreach (IClientSession member in instance.MapInstance.Sessions)
            {
                member.SendPacket(member.GenerateTaP(2, true));
                member.SendPacket("ta_s");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellation);
        }

        foreach (IClientSession session in members)
        {
            if (!e.IsCall)
            {
                newPos = session.PlayerEntity.TalentArenaComponent.TeamType switch
                {
                    ArenaTeamType.ZENAS => new Position(87, 39),
                    ArenaTeamType.ERENIA => new Position(56, 40),
                    var _ => newPos
                };
            }

            session.PlayerEntity.TeleportOnMap(newPos.X, newPos.Y);
            session.SendPacket(TalentArenaExtensions.GenerateTaSt(!e.IsCall ? TalentArenaOptionType.Call : TalentArenaOptionType.Nothing));
        }
    }
}