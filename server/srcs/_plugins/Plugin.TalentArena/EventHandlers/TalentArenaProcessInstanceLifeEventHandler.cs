using PhoenixLib.Events;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game._enum;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaProcessInstanceLifeEventHandler : IAsyncEventProcessor<TalentArenaProcessInstanceLifeEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly ITalentArenaManager _talentArenaManager;

    public TalentArenaProcessInstanceLifeEventHandler(IBuffFactory buffFactory, ITalentArenaManager talentArenaManager)
    {
        _buffFactory = buffFactory;
        _talentArenaManager = talentArenaManager;
    }

    public async Task HandleAsync(TalentArenaProcessInstanceLifeEvent e, CancellationToken cancellation)
    {
        DateTime currentTime = DateTime.UtcNow;

        await TryFinish(e.Instance);

        if (!e.Instance.Finished)
        {
            foreach (IClientSession member in e.Instance.MapInstance.Sessions)
            {
                member.SendPacket(member.GenerateTaPs());
            }

            await BuffTeam(e.Instance.ZenasTeam);
            await BuffTeam(e.Instance.EreniaTeam);
            await CheckFightTimingExpired(e.Instance, currentTime);
        }
    }

    private async Task CheckFightTimingExpired(TalentArenaInstance instance, DateTime currentTime)
    {
        DateTime time = instance.EventDateTick + instance.SecondsAmount;

        if (time >= currentTime)
        {
            return;
        }

        if (!instance.IsFighting)
        {
            return;
        }

        instance.IsFighting = false;
        var membersInFight = (List<IClientSession>)_talentArenaManager.GetTeamMembers(instance);

        // Include members called that are in fight zone
        membersInFight.AddRange(instance.MapInstance.Sessions.Where(s => s.PlayerEntity.TalentArenaComponent.IsSummoned));
        foreach (IClientSession member in membersInFight)
        {
            ITalentArenaComponent talentArenaComponent = member.PlayerEntity.TalentArenaComponent;

            if (talentArenaComponent.IsDead)
            {
                continue;
            }

            if (talentArenaComponent.IsSummoned)
            {
                member.CurrentMapInstance.Broadcast($"ta_t 0 {member.PlayerEntity.Id} 0");
                talentArenaComponent.IsSummoned = false;
            }

            await TeleportToLobby(member);
        }
    }

    private async Task TryFinish(TalentArenaInstance instance)
    {
        if (instance.Finished)
        {
            return;
        }

        // TODO: Enum
        TafTeamType teamWinner = TafTeamType.Null;
        int zenasDeaths = instance.ZenasTeam.Members.Count(s => s.PlayerEntity.TalentArenaComponent.IsDead);
        int ereniaDeaths = instance.EreniaTeam.Members.Count(s => s.PlayerEntity.TalentArenaComponent.IsDead);
        int ereniaTeamCount = instance.EreniaTeam.Members.Count;
        int zenasTeamCount = instance.ZenasTeam.Members.Count;

        if (ereniaDeaths == ereniaTeamCount && zenasDeaths == zenasTeamCount)
        {
            // Tie
            teamWinner = TafTeamType.Tie;
        }
        else if (zenasDeaths == zenasTeamCount)
        {
            // Erenia Win
            teamWinner = TafTeamType.Erenia;
        }
        else if (ereniaDeaths == ereniaTeamCount)
        {
            // Zenas Win
            teamWinner = TafTeamType.Zenas;
        }

        if (teamWinner != TafTeamType.Null)
        {
            instance.Finished = true;
            foreach (IClientSession session in instance.MapInstance.Sessions)
            {
                session.SendPacket(session.GenerateTaF((byte)teamWinner));
            }

            switch (teamWinner)
            {
                case TafTeamType.Zenas: // Zenas Win
                    foreach (IClientSession session in instance.EreniaTeam.Members)
                    {
                        session.PlayerEntity.TalentLose++;
                    }
                    foreach (IClientSession session in instance.ZenasTeam.Members)
                    {
                        session.PlayerEntity.TalentWin++;
                    }
                    break;

                case TafTeamType.Erenia: // Erenia Win
                    foreach (IClientSession session in instance.EreniaTeam.Members)
                    {
                        session.PlayerEntity.TalentWin++;
                    }
                    foreach (IClientSession session in instance.ZenasTeam.Members)
                    {
                        session.PlayerEntity.TalentLose++;
                    }
                    break;

                default:
                    return;
            }
        }
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

    private async Task BuffTeam(TalentArenaTeam team)
    {
        if (CheckStrongCombination(team.Members))
        {
            foreach (IClientSession session in team.Members)
            {
                if (session.PlayerEntity.HasBuff(BuffVnums.STRONG_COMBINATION))
                {
                    return;
                }

                Buff angry = _buffFactory.CreateBuff((short)BuffVnums.STRONG_COMBINATION, session.PlayerEntity);
                await session.PlayerEntity.AddBuffAsync(angry);
            }

            return;
        }

        if (CheckWeakCombination(team.Members))
        {
            foreach (IClientSession session in team.Members)
            {
                if (session.PlayerEntity.HasBuff(BuffVnums.WEAK_COMBINATION))
                {
                    return;
                }

                Buff angry = _buffFactory.CreateBuff((short)BuffVnums.WEAK_COMBINATION, session.PlayerEntity);
                await session.PlayerEntity.AddBuffAsync(angry);
            }
        }
    }

    private bool CheckWeakCombination(IReadOnlyList<IClientSession> members)
    {
        return members.Count(ch => ch.PlayerEntity.Class == ClassType.Archer) == 3 ||
            members.Count(ch => ch.PlayerEntity.Class == ClassType.Magician) == 3 ||
            members.Count(ch => ch.PlayerEntity.Class == ClassType.Swordman) == 3;
    }

    private bool CheckStrongCombination(IReadOnlyList<IClientSession> members)
    {
        return members.Any(ch => ch.PlayerEntity.Class == ClassType.Archer) &&
            members.Any(ch => ch.PlayerEntity.Class == ClassType.Magician) &&
            members.Any(ch => ch.PlayerEntity.Class == ClassType.Swordman);
    }
}