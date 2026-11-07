using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaInstanceStartEvent : IAsyncEvent
{
    public TalentArenaInstanceStartEvent(IReadOnlyList<IClientSession> zenasTeam, IReadOnlyList<IClientSession> ereniaTeam)
    {
        ZenasTeam = zenasTeam;
        EreniaTeam = ereniaTeam;
    }

    public IReadOnlyList<IClientSession> ZenasTeam { get; }
    public IReadOnlyList<IClientSession> EreniaTeam { get; }
}