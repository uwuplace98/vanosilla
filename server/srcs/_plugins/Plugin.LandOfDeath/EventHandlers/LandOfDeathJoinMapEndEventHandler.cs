using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;

namespace Plugin.LandOfDeath.EventHandlers;

public class LandOfDeathJoinMapEndEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
{
    private readonly ILandOfDeathManager _landOfDeathManager;

    public LandOfDeathJoinMapEndEventHandler(ILandOfDeathManager landOfDeathManager) => _landOfDeathManager = landOfDeathManager;
    
    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance)
        {
            return;
        }

        e.Sender.SendTsClockPacket(_landOfDeathManager.GetRemainingTime(), true);
    }
}