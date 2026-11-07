using System;
using System.Collections.Generic;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    /// <summary>
    /// Main Battle Pass Configuration - loaded from battlepass_configuration.yaml
    /// </summary>
    public class BattlePassMainConfiguration
    {
        public int MaxBattlePassPoints { get; set; } = 1500;
        public bool IsBattlePassEnabled { get; set; } = true;
        public DateTime StartSeason { get; set; }
        public DateTime EndSeason { get; set; }
        public int BattlePassSeason { get; set; } = 1;
    }

    /// <summary>
    /// Battle Pass Bearings (Tiers) - loaded from battlepass_bearing.yaml
    /// </summary>
    public class BattlePassBearingConfiguration
    {
        public List<BattlePassBearing> Bearings { get; set; } = new();
    }

    public class BattlePassBearing
    {
        public int Id { get; set; }
        public int MinimumBattlepassPoint { get; set; }
        public int MaximumBattlepassPoint { get; set; }

        // Normal (free) reward
        public int RewardVnum { get; set; }
        public int RewardAmount { get; set; } = 1;

        // Premium reward
        public int PremiumRewardVnum { get; set; }
        public int PremiumRewardAmount { get; set; } = 1;
        public bool IsSuperReward { get; set; } = false;
    }

    /// <summary>
    /// Battle Pass Quests - loaded from battlepass_quest.yaml
    /// </summary>
    public class BattlePassQuestConfiguration
    {
        public List<BattlePassQuest> Quests { get; set; } = new();
    }

    public class BattlePassQuest
    {
        public int Id { get; set; }
        public int MissionType { get; set; }
        public int FrequencyType { get; set; } // 0=Daily, 1=Weekly, 2=Seasonal
        public int FirstData { get; set; }
        public int MaxObjectiveValue { get; set; }
        public int RewardAmount { get; set; }
        public DateTime Start { get; set; }
    }

    // Keep old enum for backward compatibility
    public enum BattlePassQuestType
    {
        Daily = 0,
        Weekly = 1,
        Season = 2
    }

    public enum BattlePassMissionSubType
    {
        Any = 0,
        MonsterHunt = 1,
        ItemCollect = 2,
        NpcTalk = 3,
        MapVisit = 4,
        RaidComplete = 5,
        TimeSpaceComplete = 6
    }
}
