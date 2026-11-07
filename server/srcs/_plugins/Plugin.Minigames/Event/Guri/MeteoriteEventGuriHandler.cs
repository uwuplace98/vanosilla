using System.Threading.Tasks;
using Plugin.Minigames.Managers;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;

namespace Plugin.Minigames.Event.Guri
{
    public class MeteoriteEventGuriHandler : IGuriHandler
    {
        private readonly IMeteoriteGameManager _manager;

        public MeteoriteEventGuriHandler(IMeteoriteGameManager manager)
        {
            _manager = manager;
        }

        public long GuriEffectId => 507; // Changed from 506 to avoid InstantBattle conflict

        public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
        {
            await _manager.JoinGame(session);
        }
    }
}
