using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Maps;

namespace Plugin.LandOfDeath.RecurrentJob;

public class LandOfDeathSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ILandOfDeathManager _landOfDeathManager;
    private readonly IMapManager _mapManager;
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;

    public LandOfDeathSystem(IAsyncEventPipeline eventPipeline, ILandOfDeathManager landOfDeathManager, IMapManager mapManager, LandOfDeathConfiguration landOfDeathConfiguration)
    {
        _eventPipeline = eventPipeline;
        _landOfDeathManager = landOfDeathManager;
        _mapManager = mapManager;
        _landOfDeathConfiguration = landOfDeathConfiguration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[LAND_OF_DEATH_SYSTEM] Start Land of death system...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MainProcess();
            }
            catch (Exception e)
            {
                Log.Error("[LAND_OF_DEATH_SYSTEM] ", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task MainProcess()
    {
        DateTime dateNow = DateTime.UtcNow;

        switch (_landOfDeathManager.IsActive)
        {
            case false:
                ProcessStart();
                break;
            case true:
                ProcessLife(dateNow);
                break;
        }

    }

    private void ProcessLife(DateTime dateNow)
    {
        ProcessClose(dateNow);
        ProcessDestroy();
        TrySpawnHorn(dateNow);
        TryDespawnHorn(dateNow);
    }
    
    private void ProcessStart()
    {
        if (_landOfDeathManager.LandOfDeathProcessTime is null)
        {
            return;
        }

        _landOfDeathManager.NpcPortal = _mapManager.GetBaseMapInstanceByMapId(_landOfDeathConfiguration.ExitMapId).GetNpcByVNum(_landOfDeathConfiguration.NpcPortalVnum);

        if (_landOfDeathManager.NpcPortal == null)
        {
            return;
        }

        _landOfDeathManager.IsActive = true;
        _landOfDeathManager.IsClosed = false;
        _landOfDeathManager.NpcPortal.TemporaryEffect = (short)EffectType.LodNpcPortal;
    }

    private void ProcessClose(DateTime currentTime)
    {
        if (_landOfDeathManager.LandOfDeathProcessTime is null)
        {
            return;
        }

        if (!_landOfDeathManager.CanClose(currentTime))
        {
            return;
        }

        _landOfDeathManager.IsClosed = true;
        _landOfDeathManager.NpcPortal.TemporaryEffect = 0;
    }

    private void ProcessDestroy()
    {
        if (_landOfDeathManager.LandOfDeathProcessTime is null)
        {
            return;
        }

        if (_landOfDeathManager.GetRemainingTime().TotalSeconds > 0)
        {
            return;
        }

        foreach (IMapInstance instance in _landOfDeathManager.GetAllInstances())
        {
            _eventPipeline.ProcessEventAsync(new LandOfDeathDestroyEvent(instance));
        }
    }

    private void TrySpawnHorn(DateTime currentTime)
    {
        if (!_landOfDeathManager.CanSpawnHorn(currentTime))
        {
            return;
        }

        _landOfDeathManager.LastHornSpawn = currentTime;
        _landOfDeathManager.ProcessingHorn = true;
        foreach (IMapInstance instance in _landOfDeathManager.GetAllInstances())
        {
            _eventPipeline.ProcessEventAsync(new LandOfDeathSpawnHornEvent(instance, true));
        }
    }

    private void TryDespawnHorn(DateTime currentTime)
    {
        if (!_landOfDeathManager.ProcessingHorn)
        {
            return;
            
        }
        
        if (!_landOfDeathManager.CanDespawnHorn(currentTime))
        {
            return;
        }

        _landOfDeathManager.ProcessingHorn = false;
        _landOfDeathManager.LastHornSpawn = currentTime;

        foreach (IMapInstance instance in _landOfDeathManager.GetAllInstances())
        {
            _eventPipeline.ProcessEventAsync(new LandOfDeathSpawnHornEvent(instance, false));
        }
    }
}