using PhoenixLib.Events;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.LandOfDeath.Events;

public class LandOfDeathDestroyEvent : IAsyncEvent
{
    public LandOfDeathDestroyEvent(IMapInstance instance) => Instance = instance;

    public IMapInstance Instance { get; }
}