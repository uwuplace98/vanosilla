using System;
using System.Collections.Generic;
using System.Threading;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Arena.TalentArena;

public class TalentArenaInstance
{
    private readonly ReaderWriterLockSlim _lock = new();


    public TalentArenaInstance(TalentArenaTeam zenasTeam, TalentArenaTeam ereniaTeam)
    {
        EreniaTeam = ereniaTeam;
        ZenasTeam = zenasTeam;
        EventDateTick = DateTime.UtcNow;
    }

    public IMapInstance MapInstance { get; init; }
    public TalentArenaTeam ZenasTeam { get; }
    public TalentArenaTeam EreniaTeam { get; }

    public byte Round { get; set; }
    public DateTime EventDateTick { get; set; }
    public TimeSpan SecondsAmount { get; set; }
    public bool IsFighting { get; set; }
    public bool Finished { get; set; }
}