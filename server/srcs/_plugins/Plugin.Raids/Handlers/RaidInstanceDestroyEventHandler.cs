using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Revival;

namespace Plugin.Raids;

public class RaidInstanceDestroyEventHandler : IAsyncEventProcessor<RaidInstanceDestroyEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IRaidManager _raidManager;
    private readonly IAct4CaligorManager _act4CaligorManager;

    public RaidInstanceDestroyEventHandler(IRaidManager raidManager, IMapManager mapManager, IAct4CaligorManager act4CaligorManager)
    {
        _raidManager = raidManager;
        _mapManager = mapManager;
        _act4CaligorManager = act4CaligorManager;
    }

    public async Task HandleAsync(RaidInstanceDestroyEvent e, CancellationToken cancellation)
    {
        Log.Warn("Destroying raid instance");
        _raidManager.RemoveRaid(e.RaidParty);

        if (e.RaidParty.Instance == null)
        {
            return;
        }

        foreach (RaidSubInstance subInstance in e.RaidParty.Instance.RaidSubInstances.Values)
        {
            foreach (IClientSession session in subInstance.MapInstance.Sessions.ToList())
            {
                RaidPartyLeaveEventHandler.InternalLeave(session);

                if (!session.PlayerEntity.IsAlive())
                {
                    await session.EmitEventAsync(new RevivalReviveEvent());
                }

                if (e.RaidParty.Type == RaidType.Fernon)
                {
                    if (_act4CaligorManager.FernonMapsActive)
                    {
                        short x = session.PlayerEntity.PositionBeforeFernonRaidEnter.X;
                        short y = session.PlayerEntity.PositionBeforeFernonRaidEnter.Y;
                        session.ChangeMap(_act4CaligorManager.FernonMap, x, y);
                    }
                    else
                    {
                        session.ChangeMap(153, 93, 93);
                    }
                }
                else
                {
                    session.ChangeToLastBaseMap();
                }
            }

            _mapManager.RemoveMapInstance(subInstance.MapInstance.Id);
            subInstance.MapInstance.Destroy();
        }

        e.RaidParty.Destroy = true;
    }
}