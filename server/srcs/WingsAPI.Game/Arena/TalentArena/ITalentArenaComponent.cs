using System;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Arena.TalentArena;

public interface ITalentArenaComponent
{
    TalentArenaInstance Instance { get; }
    bool IsInTalentArena { get; }
    public long GroupId { get; set; }
    ArenaTeamType TeamType { get; set; }

    ArenaEventType ArenaType { get; }

    ArenaMemberStateType StateType { get; set; }

    ushort Time { get; set; }

    int Order { get; set; }

    bool IsDead { get; set; }

    bool IsSummoned { get; set; }
    DateTime LastSummon { get; set; }
    bool IsInFightZone { get; set; }
    IClientSession Killer { get; set; }

    void SetTalentArena(TalentArenaInstance instance, ArenaTeamType team);
    void RemoveTalentArena();
    void RegisterToTalentArena();
}

public class TalentArenaComponent : ITalentArenaComponent
{
    public bool IsInTalentArena => Instance != null;
    public TalentArenaInstance Instance { get; private set; }
    public ArenaTeamType TeamType { get; set; }
    public long GroupId { get; set; }

    public ArenaEventType ArenaType { get; set; }

    public ArenaMemberStateType StateType { get; set; }

    public ushort Time { get; set; }

    public int Order { get; set; }

    public bool IsDead { get; set; }

    public bool IsSummoned { get; set; }

    public DateTime LastSummon { get; set; }

    public bool IsInFightZone { get; set; }

    public IClientSession Killer { get; set; }

    public void SetTalentArena(TalentArenaInstance instance, ArenaTeamType team)
    {
        Instance = instance;
        TeamType = team;
    }

    public void RemoveTalentArena()
    {
        Instance = null;
        IsSummoned = false;
        IsDead = false;
        Killer = null;
        IsInFightZone = false;
    }

    public void RegisterToTalentArena()
    {
        ArenaType = ArenaEventType.TalentArena;
        StateType = ArenaMemberStateType.TeamSearch;
        GroupId = -1;
        Time = 300;
        Order = -1;
        LastSummon = DateTime.UtcNow;
    }
}