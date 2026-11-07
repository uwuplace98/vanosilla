using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Helpers.Damages; // For IsInRange
using WingsEmu.Game.Managers; // For IIcebreakerManager
using WingsEmu.Game.Networking;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Game.Extensions; // For SendMsg

namespace Plugin.Icebreaker.Event.Icebreaker.Guri
{
    public class IcebreakerUnfreezeEventHandler : IGuriHandler
    {
        public long GuriEffectId => 502;

        private readonly IIcebreakerManager _icebreakerManager;
        private readonly ISessionManager _sessionManager;

        public IcebreakerUnfreezeEventHandler(IIcebreakerManager icebreakerManager, ISessionManager sessionManager)
        {
            _icebreakerManager = icebreakerManager;
            _sessionManager = sessionManager;
        }

        public async Task ExecuteAsync(IClientSession session, GuriEvent e)
        {
            long targetId = (long)e.User;
            var targetSession = _sessionManager.GetSessionByCharacterId(targetId);

            if (targetSession == null)
            {
                return;
            }

            if (!_icebreakerManager.IsFrozen(targetId))
            {
                return;
            }

            // Distance check
            if (!session.PlayerEntity.Position.IsInRange(targetSession.PlayerEntity.Position, 5))
            {
                session.SendMsg("Too far away.", MsgMessageType.Middle);
                return;
            }

            await _icebreakerManager.Unfreeze(targetSession.PlayerEntity, session.PlayerEntity);
        }
    }
}
