using PhoenixLib.Events;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Networking;

namespace Plugin.LandOfDeath.EventHandlers;

public class LandOfDeathDestroyEventHandler : IAsyncEventProcessor<LandOfDeathDestroyEvent>
{
    private readonly ILandOfDeathManager _landOfDeathManager;

    public LandOfDeathDestroyEventHandler(ILandOfDeathManager landOfDeathManager) => _landOfDeathManager = landOfDeathManager;

    public async Task HandleAsync(LandOfDeathDestroyEvent e, CancellationToken cancellation)
    {
        _landOfDeathManager.LandOfDeathProcessTime = null;
        _landOfDeathManager.IsActive = false;
        
        // To be sure is displays closed
        _landOfDeathManager.IsClosed = true;
        _landOfDeathManager.NpcPortal.TemporaryEffect = 0;
        
        foreach (IClientSession session in e.Instance.Sessions)
        {
            await session.EmitEventAsync(new LandOfDeathLeaveMapEvent());
        }
        
        _landOfDeathManager.RemoveInstance(e.Instance);
    }
}