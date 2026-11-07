using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.LandOfDeath;

public interface ILandOfDeathManager
{
    DateTime? LandOfDeathProcessTime { get; set; }
    bool IsActive { get; set; }

    bool IsClosed { get; set; }

    bool ProcessingHorn { get; set; }

    INpcEntity NpcPortal { get; set; }

    DateTime? LastHornSpawn { get; set; }

    bool CanClose(DateTime dateTime);
    bool CanSpawnHorn(DateTime dateTime);
    bool CanDespawnHorn(DateTime dateTime);

    TimeSpan GetRemainingTime();

    IEnumerable<IMapInstance> GetAllInstances();

    IMapInstance GetInstance(Func<IMapInstance, bool> predicate);

    void AddInstance(IMapInstance instance);

    void RemoveInstance(IMapInstance instance);
}

public class LandOfDeathManager : ILandOfDeathManager
{
    private readonly TimeSpan _closeTime;
    private readonly TimeSpan _lodLifeTime;
    private readonly TimeSpan _hornSpawnTime;
    private readonly TimeSpan _hornLifeTime;
    private readonly List<IMapInstance> _lodInstances;

    public LandOfDeathManager(LandOfDeathConfiguration landOfDeathConfiguration)
    {
        _closeTime = TimeSpan.FromSeconds(landOfDeathConfiguration.ClosePortalTimeSeconds);
        _lodLifeTime = TimeSpan.FromSeconds(landOfDeathConfiguration.LodLifeTimeSeconds);
        _hornSpawnTime = TimeSpan.FromSeconds(landOfDeathConfiguration.DhSpawnTimeSeconds);
        _hornLifeTime = TimeSpan.FromSeconds(landOfDeathConfiguration.DhLifeSeconds);
        _lodInstances = new List<IMapInstance>();
    }

    public DateTime? LandOfDeathProcessTime { get; set; }

    public DateTime? LastHornSpawn { get; set; }

    public INpcEntity NpcPortal { get; set; }

    public bool ProcessingHorn { get; set; }

    public bool IsActive { get; set; }

    public bool IsClosed { get; set; }

    public TimeSpan GetRemainingTime()
    {
        if (LandOfDeathProcessTime == null)
        {
            return TimeSpan.Zero;
        }

        TimeSpan elapsedTime = DateTime.UtcNow - LandOfDeathProcessTime.Value;

        TimeSpan remainingTime = _lodLifeTime - elapsedTime;

        return remainingTime;
    }

    public bool CanClose(DateTime dateTime) => LandOfDeathProcessTime + _closeTime + _hornLifeTime < dateTime;

    public bool CanSpawnHorn(DateTime dateTime)
    {
        if (LastHornSpawn == null)
        {
            return LandOfDeathProcessTime + _hornSpawnTime < dateTime;
        }

        return LastHornSpawn + _hornSpawnTime < dateTime;
    }

    public bool CanDespawnHorn(DateTime dateTime) => LastHornSpawn + _hornLifeTime < dateTime;

    public IEnumerable<IMapInstance> GetAllInstances() => _lodInstances;

    public IMapInstance GetInstance(Func<IMapInstance, bool> predicate) => _lodInstances.FirstOrDefault(x => x != null && (predicate == null || predicate(x)));

    public void AddInstance(IMapInstance instance) => _lodInstances.Add(instance);

    public void RemoveInstance(IMapInstance instance) => _lodInstances.Remove(instance);
}