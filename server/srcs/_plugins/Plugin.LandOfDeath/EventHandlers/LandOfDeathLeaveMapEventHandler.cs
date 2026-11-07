using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace Plugin.LandOfDeath.EventHandlers;

public class LandOfDeathLeaveMapEventHandler : IAsyncEventProcessor<LandOfDeathLeaveMapEvent>
{
    private readonly IMapManager _mapManager;
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;

    public LandOfDeathLeaveMapEventHandler(IMapManager mapManager, LandOfDeathConfiguration landOfDeathConfiguration)
    {
        _mapManager = mapManager;
        _landOfDeathConfiguration = landOfDeathConfiguration;
    }

    public async Task HandleAsync(LandOfDeathLeaveMapEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance)
        {
            return;
        }
        
        if (session.PlayerEntity.Family.LandOfDeath != null)
        {
            session.PlayerEntity.Family.LandOfDeath = null;
        }
        
        if (!session.PlayerEntity.IsAlive())
        {
            session.UpdateVisibility();
            session.PlayerEntity.Hp = 1;
            session.PlayerEntity.Mp = 1;
            await session.PlayerEntity.Restore(restoreHealth: false, restoreMana: false, restoreMates: false);
            session.SendBuffsPacket();
            session.ChangeMap(_landOfDeathConfiguration.ExitMapId, _landOfDeathConfiguration.ExitPos.X, _landOfDeathConfiguration.ExitPos.Y);
            return;
        }
        
        await _mapManager.TeleportOnRandomPlaceInMapAsync(session, _mapManager.GetBaseMapInstanceByMapId(_landOfDeathConfiguration.ExitMapId));
            
        
    }
}