using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    public class BattlePassObjectiveEventHandler : IAsyncEventProcessor<QuestMonsterDeathEvent>
    {
        private readonly IBattlePassManager _battlePassManager;
        private readonly IGameLanguageService _languageService;

        public BattlePassObjectiveEventHandler(IBattlePassManager battlePassManager, IGameLanguageService languageService)
        {
            _battlePassManager = battlePassManager;
            _languageService = languageService;
        }

        public async Task HandleAsync(QuestMonsterDeathEvent e, CancellationToken cancellationToken)
        {
            IClientSession session = e.Sender;
            if (session == null || !session.HasCurrentMapInstance)
            {
                return;
            }

            var player = session.PlayerEntity;
            if (player.BattlePassQuests == null || !player.BattlePassQuests.Any())
            {
                return;
            }

            var questConfig = _battlePassManager.QuestConfig;
            if (questConfig?.Quests == null || !questConfig.Quests.Any()) return;

            bool updated = false;

            foreach (var userQuest in player.BattlePassQuests)
            {
                if (userQuest.IsFinished) continue;

                var quest = questConfig.Quests.FirstOrDefault(q => q.Id == userQuest.QuestId);
                if (quest == null) continue;

                // Check Type - MissionType 1 = MonsterHunt
                if (quest.MissionType != 1) continue;

                // Check Target (FirstData is the target monster VNum)
                if (quest.FirstData != e.MonsterEntity.MonsterVNum) continue;

                // Update Progress
                userQuest.Progress++;
                updated = true;

                // Check Completion
                if (userQuest.Progress >= quest.MaxObjectiveValue)
                {
                    userQuest.IsFinished = true;
                    player.BattlePassPoints += quest.RewardAmount;

                    // Notify User
                    session.SendChatMessage($"[Battle Pass] Quest Completed: {quest.RewardAmount} Points awarded!", ChatMessageColorType.Green);
                    session.SendMsg($"[Battle Pass] Quest Completed!", MsgMessageType.Middle);
                }
            }

            if (updated)
            {
                // Re-calculate bearing/progress
                int premium = player.BattlePassIsPremium ? 1 : 0;
                int bearingCount = player.BattlePassClaimedRewards?.Count ?? 0;
                session.SendPacket($"bpp {bearingCount} {player.BattlePassPoints} {premium}");
            }

            await Task.CompletedTask;
        }
    }
}
