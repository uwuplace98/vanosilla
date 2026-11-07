using System;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event
{
    public class Act4CaligorAddFactionPointEventHandler : IAsyncEventProcessor<Act4CaligorAddFactionPointEvent>
    {
        private readonly IAct4CaligorManager _manager;
        private readonly ISessionManager _sessionManager;

        public Act4CaligorAddFactionPointEventHandler(IAct4CaligorManager manager, ISessionManager sessionManager)
        {
            _manager = manager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(Act4CaligorAddFactionPointEvent e, CancellationToken cancellation)
        {
            if (!_manager.CaligorActive)
            {
                return;
            }

            if (e.Faction == FactionType.Angel)
            {
                _manager.AngelDamage += e.Damage;
                _manager.RefreshCaligorInstance();
                return;
            }

            _manager.DemonDamage += e.Damage;
            _manager.RefreshCaligorInstance();
        }
    }
}