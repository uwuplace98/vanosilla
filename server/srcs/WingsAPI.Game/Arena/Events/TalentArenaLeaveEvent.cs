using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Arena.Events;

public class TalentArenaLeaveEvent : PlayerEvent
{
    public bool Surrender { get; init; }
}