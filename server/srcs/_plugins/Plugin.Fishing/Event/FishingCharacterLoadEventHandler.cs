using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace Plugin.Fishing.Event
{
    public class FishingCharacterLoadEventHandler : IAsyncEventProcessor<CharacterLoadEvent>
    {
        private readonly IFishingManager _fishingManager;

        public FishingCharacterLoadEventHandler(IFishingManager fishingManager)
        {
            _fishingManager = fishingManager;
        }

        public async Task HandleAsync(CharacterLoadEvent e, CancellationToken cancellation)
        {
            var session = e.Sender;
            if (session == null) return;

            // Send initial fish packet
            session.SendPacket(_fishingManager.GenerateFishPacket(session, FishPacketType.Login));
        }
    }
}
