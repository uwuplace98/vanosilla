using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Managers; // For IIcebreakerManager
using WingsEmu.Game.Networking;

namespace Plugin.Icebreaker.Event.Icebreaker.Guri
{
    public class IcebreakerJoinEventHandler : IGuriHandler
    {
        public long GuriEffectId => 501;

        private readonly IIcebreakerManager _icebreakerManager;

        public IcebreakerJoinEventHandler(IIcebreakerManager icebreakerManager)
        {
            _icebreakerManager = icebreakerManager;
        }

        public async Task ExecuteAsync(IClientSession session, GuriEvent e)
        {
            await _icebreakerManager.Join(session.PlayerEntity);
        }
    }
}
