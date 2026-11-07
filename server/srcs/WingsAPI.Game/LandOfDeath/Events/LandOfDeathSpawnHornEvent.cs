using PhoenixLib.Events;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.LandOfDeath.Events;

public class LandOfDeathSpawnHornEvent : IAsyncEvent
{
    public LandOfDeathSpawnHornEvent(IMapInstance instance, bool shouldSpawn)
    {
        MapInstance = instance;
        ShouldSpawn = shouldSpawn;
    }

    public IMapInstance MapInstance { get; }
    public bool ShouldSpawn { get; }
}