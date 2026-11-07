using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaProcessInstanceLifeEvent : IAsyncEvent
{
    public TalentArenaInstance Instance { get; init; }
}