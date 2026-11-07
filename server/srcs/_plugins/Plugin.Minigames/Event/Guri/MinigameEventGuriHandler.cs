using System;
using System.Threading.Tasks;
using Plugin.Minigames.Managers;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Minigames.Event.Guri
{
    /// <summary>
    /// Unified guri handler for minigame joins and InstantBattle registration.
    /// OpenNOS uses guri 506 for all minigames (Sheep Farm, Meteorite, Instant Battle).
    /// The client sends guri 506 when clicking "Join" on the bsinfo/qnaml dialog.
    /// </summary>
    public class MinigameEventGuriHandler : IGuriHandler
    {
        private readonly ISheepFarmManager _sheepFarmManager;
        private readonly IMeteoriteGameManager _meteoriteGameManager;
        private readonly IGameEventRegistrationManager _gameEventRegistrationManager;
        private readonly IGameLanguageService _language;

        public MinigameEventGuriHandler(
            ISheepFarmManager sheepFarmManager,
            IMeteoriteGameManager meteoriteGameManager,
            IGameEventRegistrationManager gameEventRegistrationManager,
            IGameLanguageService language)
        {
            _sheepFarmManager = sheepFarmManager;
            _meteoriteGameManager = meteoriteGameManager;
            _gameEventRegistrationManager = gameEventRegistrationManager;
            _language = language;
        }

        public long GuriEffectId => 506;

        public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
        {
            PhoenixLib.Logging.Log.Info($"[MINIGAME_GURI_506] Player {session.PlayerEntity.Name} sent guri 506 - Data: {guriPacket.Data}, Value: {guriPacket.Value}");

            // Priority 1: Check if InstantBattle registration is open
            if (_gameEventRegistrationManager.IsGameEventRegistrationOpen(GameEventType.InstantBattle, DateTime.UtcNow))
            {
                if (session.IsMuted())
                {
                    session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
                    return;
                }
                
                _gameEventRegistrationManager.SetCharacterGameEventInclination(session.PlayerEntity.Id, GameEventType.InstantBattle);
                session.SendChatMessage("Registered for Instant Battle!", ChatMessageColorType.Green);
                return;
            }

            // Priority 2: Meteorite game is running
            if (_meteoriteGameManager.IsGameRunning)
            {
                await _meteoriteGameManager.JoinGame(session);
                return;
            }

            // Priority 3: Sheep Farm game is running
            if (_sheepFarmManager.IsGameRunning)
            {
                await _sheepFarmManager.JoinGame(session);
                return;
            }

            // If no game is running, start a new Sheep Farm game (default)
            await _sheepFarmManager.JoinGame(session);
        }
    }
}
