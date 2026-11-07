using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaCallEvent : IAsyncEvent
{
    public TalentArenaCallEvent(IClientSession sender, IClientSession memberCalled, TalentArenaTeam team)
    {
        Sender = sender;
        Client = memberCalled;
        Team = team;
    }

    public IClientSession Sender { get; }
    public IClientSession Client { get; }
    public TalentArenaTeam Team { get; }
}