using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaFinishRoundEvent : IAsyncEvent
{
    public TalentArenaFinishRoundEvent(TalentArenaInstance instance) => Instance = instance;

    public TalentArenaInstance Instance { get; }
}