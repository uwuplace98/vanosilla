using PhoenixLib.Events;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaLeaveEventHandler : IAsyncEventProcessor<TalentArenaLeaveEvent>
{
    private readonly IArenaManager _arenaManager;
    private readonly ITalentArenaManager _talentArenaManager;
    private readonly IMapManager _mapManager;

    public TalentArenaLeaveEventHandler(ITalentArenaManager talentArenaManager, IMapManager mapManager, IArenaManager arenaManager)
    {
        _talentArenaManager = talentArenaManager;
        _mapManager = mapManager;
        _arenaManager = arenaManager;
    }

    public async Task HandleAsync(TalentArenaLeaveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        ITalentArenaComponent sessionAotComponent = session.PlayerEntity.TalentArenaComponent;
        if (!sessionAotComponent.IsInTalentArena)
        {
            if (!_talentArenaManager.TryUnregisterLobbyMember(session))
            {
                return;
            }
            session.SendBsInfoPacket(BsInfoType.CloseWindow, GameType.ArenaOfTalents, 0, QueueWindow.WaitForEntry);

            var sessions = _talentArenaManager
                .GetRegisteredMembers(ArenaEventType.TalentArena, s => s.PlayerEntity.TalentArenaComponent.GroupId == sessionAotComponent.GroupId).ToList();

            if (sessions.Count < 1)
            {
                return;
            }

            foreach (IClientSession member in sessions)
            {
                ITalentArenaComponent memberAotComponent = member.PlayerEntity.TalentArenaComponent;
                if (sessions.Count < 2)
                {
                    memberAotComponent.GroupId = -1;
                }

                memberAotComponent.Time = 300;
                memberAotComponent.StateType = ArenaMemberStateType.TeamSearch;
                member.SendBsInfoPacket(BsInfoType.OpenWindow, GameType.ArenaOfTalents, 0, QueueWindow.SearchRegisteredTeam);
            }

            return;
        }

        TalentArenaInstance talentArenaInstance = sessionAotComponent.Instance;

        // if (e.CheckIfFinished && talentArenaInstance.FinishTime == null)
        // {
        //     return;
        // }

        ArenaTeamType team = sessionAotComponent.TeamType;

        switch (team)
        {
            case ArenaTeamType.ZENAS:
                talentArenaInstance.ZenasTeam.Members.Remove(session);
                break;

            case ArenaTeamType.ERENIA:
                talentArenaInstance.EreniaTeam.Members.Remove(session);
                break;
        }

        if (e.Surrender)
        {
            session.PlayerEntity.TalentSurrender++;
        }

        session.SendPacket(session.GenerateTaM(1));
        session.SendPacket(session.GenerateTaP(1, true));
        session.SendPacket(TalentArenaExtensions.GenerateTaSv(1));
        session.SendPacket("taw_sv 1");

        sessionAotComponent.RemoveTalentArena();

        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;

        await session.PlayerEntity.RemovePositiveBuffs(100);
        await session.PlayerEntity.RemoveNegativeBuffs(100);

        session.PlayerEntity.ClearSkillCooldowns();
        foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
        {
            skill.LastUse = DateTime.MinValue;
            session.SendSkillCooldownReset(skill.Skill.CastId);
        }

        await _mapManager.TeleportOnRandomPlaceInMapAsync(session, _arenaManager.ArenaInstance);

        foreach (IClientSession remainingMembers in talentArenaInstance.MapInstance.Sessions)
        {
            // TODO: Log "Player Left"
            remainingMembers.SendPacket(remainingMembers.GenerateTaP(2, true));
        }
    }
}