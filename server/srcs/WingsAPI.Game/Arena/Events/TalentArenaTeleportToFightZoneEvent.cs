using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaTeleportToFightZoneEvent : IAsyncEvent
{
    public TalentArenaTeleportToFightZoneEvent(TalentArenaInstance instance, bool isCall)
    {
        ArenaInstance = instance;
        IsCall = isCall;
    }

    public TalentArenaInstance ArenaInstance { get; }

    public Position Position { get; set; }

    public bool IsCall { get; }

    public IClientSession MemberCalled { get; set; }
}