using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.TalentArena.Managers;

public interface ITalentArenaFactory
{
    Task<TalentArenaInstance> CreateTalentArenaInstance(IReadOnlyList<IClientSession> zenasTeam, IReadOnlyList<IClientSession> ereniaTeam);
}

public class TalentArenaFactory : ITalentArenaFactory
{
    private readonly IMapManager _mapManager;
    private readonly ITalentArenaManager _talentArenaManager;

    public TalentArenaFactory(IMapManager mapManager, ITalentArenaManager talentArenaManager)
    {
        _mapManager = mapManager;
        _talentArenaManager = talentArenaManager;
    }

    public async Task<TalentArenaInstance> CreateTalentArenaInstance(IReadOnlyList<IClientSession> zenasTeamMembers, IReadOnlyList<IClientSession> ereniaTeamMembers)
    {
        IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(2015, MapInstanceType.TalentArenaInstance);
        if (mapInstance == null)
        {
            return null;
        }

        mapInstance.Initialize(DateTime.UtcNow.AddSeconds(-1));

        var zenasTeam = new TalentArenaTeam();
        var ereniaTeam = new TalentArenaTeam();

        SetMemberToTeam(zenasTeamMembers, zenasTeam, ArenaTeamType.ZENAS);
        SetMemberToTeam(ereniaTeamMembers, ereniaTeam, ArenaTeamType.ERENIA);

        var arenaInstance = new TalentArenaInstance(zenasTeam, ereniaTeam)
        {
            MapInstance = mapInstance
        };

        return arenaInstance;
    }

    private void SetMemberToTeam(IEnumerable<IClientSession> members, TalentArenaTeam team, ArenaTeamType teamType)
    {
        foreach (IClientSession member in members)
        {
            member.SendBsInfoPacket(BsInfoType.CloseWindow, GameType.ArenaOfTalents, 0, QueueWindow.WaitForEntry);
            member.PlayerEntity.TalentArenaComponent.TeamType = teamType;
            team.Members.Add(member);
            _talentArenaManager.TryUnregisterLobbyMember(member);
        }
    }
}