using System;
using System.Linq;
using WingsEmu.Game.Characters;
using WingsEmu.Plugins.BasicImplementation.BattlePass;

namespace WingsEmu.Plugins.BasicImplementation.Extensions
{
    public static class BattlePassExtensions
    {
        /// <summary>
        /// Generates the bpm packet containing quest information.
        /// Format: bpm {questCount} {iconEnabled} {maxPoints} {startDate} {endDate} {quests...}
        /// Expected quest format from reference:
        /// {id} {type(missionType)} {frequency(frequencyType)} {isTaken} {maxObjectiveValue} {firstData/progress} {rewardAmount} {time} {rewardType}
        /// </summary>
        public static string GenerateBpm(this IBattlePassManager manager, IPlayerEntity character)
        {
            var mainConf = manager.MainConfig;
            var questConf = manager.QuestConfig;

            if (mainConf == null || questConf?.Quests == null || !questConf.Quests.Any())
            {
                return null;
            }

            long startSeason = long.Parse(mainConf.StartSeason.ToString("yyyyMMdd"));
            long endSeason = long.Parse(mainConf.EndSeason.ToString("yyyyMMdd"));

            string quests = "";
            foreach (var quest in questConf.Quests)
            {
                // Calculate remaining time based on frequency type
                TimeSpan timeSpan = TimeSpan.Zero;
                switch ((BattlePassQuestType)quest.FrequencyType)
                {
                    case BattlePassQuestType.Daily:
                        timeSpan = DateTime.Today.AddDays(1) - DateTime.Now;
                        break;
                    case BattlePassQuestType.Weekly:
                        int daysUntilEndOfWeek = ((int)DayOfWeek.Sunday - (int)DateTime.Now.DayOfWeek + 7) % 7;
                        if (daysUntilEndOfWeek == 0) daysUntilEndOfWeek = 7;
                        timeSpan = DateTime.Today.AddDays(daysUntilEndOfWeek) - DateTime.Now;
                        break;
                    case BattlePassQuestType.Season:
                        timeSpan = mainConf.EndSeason - DateTime.Now;
                        break;
                }

                // Find player's progress for this quest
                var userQuest = character.BattlePassQuests?.FirstOrDefault(q => q.QuestId == quest.Id);
                int progress = (int)(userQuest?.Progress ?? 0);
                bool alreadyTaken = userQuest?.IsFinished ?? false;
                int isTaken = alreadyTaken ? 1 : 0;
                int remainingMinutes = timeSpan.TotalMinutes < 0 ? -1 : (int)Math.Round(timeSpan.TotalMinutes);
                int rewardType = 0; // 0=points

                // Format: {id} {missionType} {frequencyType} {isTaken} {maxObjectiveValue} {firstData/progress} {rewardAmount} {time} {rewardType}
                quests += $"{quest.Id} {quest.MissionType} {quest.FrequencyType} {isTaken} {quest.MaxObjectiveValue} {progress} {quest.RewardAmount} {remainingMinutes} {rewardType} ";
            }

            // bpm {questCount} {iconEnabled} {maxPoints} {startDate} {endDate} {quests}
            int iconEnabled = 1;
            return $"bpm {questConf.Quests.Count} {iconEnabled} {mainConf.MaxBattlePassPoints} {startSeason} {endSeason} {quests.TrimEnd(' ')}";
        }

        /// <summary>
        /// Generates the bpp packet containing tier/bearing information.
        /// Format: bpp {bearingCount} {playerPoints} {hasPremium} {bearings...}
        /// </summary>
        public static string GenerateBpp(this IBattlePassManager manager, IPlayerEntity character)
        {
            var bearingConf = manager.BearingConfig;
            if (bearingConf?.Bearings == null || !bearingConf.Bearings.Any())
            {
                int premium = character.BattlePassIsPremium ? 1 : 0;
                return $"bpp 0 {character.BattlePassPoints} {premium}";
            }

            string bearings = "";
            foreach (var bearing in bearingConf.Bearings)
            {
                // Check if player can claim this bearing (has enough points)
                bool canGetItem = character.BattlePassPoints >= bearing.MinimumBattlepassPoint;

                // Check if free/premium rewards are already claimed
                // Note: Currently using same list for both - could be separated later
                bool freeItemAlreadyTaken = character.BattlePassClaimedRewards?.Contains(bearing.Id) ?? false;
                bool premiumItemAlreadyTaken = character.BattlePassClaimedRewards?.Contains(bearing.Id + 10000) ?? false;

                // Status: 0=locked, 1=available, 2=claimed
                int freeStatus = !canGetItem ? 0 : (freeItemAlreadyTaken ? 2 : 1);
                int premiumStatus = !canGetItem ? 0 : (premiumItemAlreadyTaken ? 2 : 1);

                // Format: {id} {freeVnum} {freeAmount} {premiumVnum} {premiumAmount} {freeStatus} {premiumStatus} {isSuperReward}
                bearings += $"{bearing.Id} {bearing.RewardVnum} {bearing.RewardAmount} " +
                           $"{bearing.PremiumRewardVnum} {bearing.PremiumRewardAmount} " +
                           $"{freeStatus} {premiumStatus} {(bearing.IsSuperReward ? 1 : 0)} ";
            }

            int hasPremium = character.BattlePassIsPremium ? 1 : 0;
            return $"bpp {bearingConf.Bearings.Count} {character.BattlePassPoints} {hasPremium} {bearings.TrimEnd(' ')}";
        }

        /// <summary>
        /// Simple GenerateBpp for character only (backward compatibility).
        /// </summary>
        public static string GenerateBpp(this IPlayerEntity character)
        {
            int premium = character.BattlePassIsPremium ? 1 : 0;
            int bearingCount = character.BattlePassClaimedRewards?.Count ?? 0;
            return $"bpp {bearingCount} {character.BattlePassPoints} {premium}";
        }

        /// <summary>
        /// Generates the bpt packet containing remaining season time.
        /// Format: bpt {remainingMinutes}
        /// </summary>
        public static string GenerateBpt(this IBattlePassManager manager)
        {
            var mainConf = manager.MainConfig;
            if (mainConf == null)
            {
                return null;
            }

            TimeSpan timeSpan = mainConf.EndSeason - DateTime.Now;
            return $"bpt {Math.Round(timeSpan.TotalMinutes)}";
        }
    }
}
