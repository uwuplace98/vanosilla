using System.Threading.Tasks;
using Plugin.Minigames.Managers;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;

namespace Plugin.Minigames.Event.Guri
{
    public class SheepEventGuriHandler : IGuriHandler
    {
        private readonly ISheepFarmManager _manager;

        public SheepEventGuriHandler(ISheepFarmManager manager)
        {
            _manager = manager;
        }

        public long GuriEffectId => 514;

        public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
        {
            await _manager.JoinGame(session);
        }
    }
}
