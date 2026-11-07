using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.BasicImplementation.Extensions;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    /// <summary>
    /// Handles bp_msel packet - Claim mission (quest) reward.
    /// Reference: When a quest is complete, player clicks to claim the points reward.
    /// </summary>
    public class BpMSelPacketHandler : GenericGamePacketHandlerBase<BpMSelPacket>
    {
        private readonly IBattlePassManager _manager;

        public BpMSelPacketHandler(IBattlePassManager manager)
        {
            _manager = manager;
        }

        protected override async Task HandlePacketAsync(IClientSession session, BpMSelPacket packet)
        {
            await Task.CompletedTask;

            PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_MISSION] Received Request: QuestId={packet.QuestId}");

            var character = session.PlayerEntity;
            var questConf = _manager.QuestConfig;

            if (questConf?.Quests == null || !questConf.Quests.Any())
            {
                PhoenixLib.Logging.Log.Warn("[BATTLEPASS_MISSION] Configuration not loaded.");
                return;
            }

            // Find quest definition
            var questDef = questConf.Quests.FirstOrDefault(q => q.Id == packet.QuestId);
            if (questDef == null)
            {
                PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_MISSION] Quest {packet.QuestId} not found in config.");
                return;
            }

            // Find player's progress for this quest
            var userQuest = character.BattlePassQuests?.FirstOrDefault(q => q.QuestId == packet.QuestId);
            if (userQuest == null)
            {
                PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_MISSION] Player doesn't have quest {packet.QuestId}.");
                return;
            }

            // Check if already claimed
            if (userQuest.IsFinished)
            {
                PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_MISSION] Quest {packet.QuestId} already claimed.");
                return;
            }

            // Check if quest is complete
            if (userQuest.Progress < questDef.MaxObjectiveValue)
            {
                PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_MISSION] Quest {packet.QuestId} not complete. Progress: {userQuest.Progress}/{questDef.MaxObjectiveValue}");
                return;
            }

            // Grant points reward
            character.BattlePassPoints += questDef.RewardAmount;
            userQuest.IsFinished = true;

            session.SendChatMessage($"You received {questDef.RewardAmount} Battle Pass points!", ChatMessageColorType.Yellow);

            // Send updated packets
            session.SendPacket(_manager.GenerateBpp(character));
            session.SendPacket(_manager.GenerateBpm(character));

            PhoenixLib.Logging.Log.Info($"[BATTLEPASS_MISSION] Player {character.Name} claimed quest {packet.QuestId} for {questDef.RewardAmount} points.");
        }
    }
}
