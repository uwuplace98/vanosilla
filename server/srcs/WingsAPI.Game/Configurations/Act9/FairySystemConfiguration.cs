using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act9
{
    public class FairySystemItem
    {
        public int Vnum { get; set; }
        public int Amount { get; set; }
    }

    // =========================
    // UPGRADE (PER LEVEL)
    // =========================

    public class FairyUpgradeLevelConfiguration
    {
        public int SuccessChance { get; set; } // %
        public int FailChance { get; set; }    // %
        public int LevelFixed { get; set; }

        public int GoldCost { get; set; }
        public List<FairySystemItem> Items { get; set; }
    }

    public class FairyUpgradeConfiguration
    {
        /// <summary>
        /// Key = enchant level (1..9)
        /// </summary>
        public Dictionary<int, FairyUpgradeLevelConfiguration> Levels { get; set; }
    }

    // =========================
    // ENCHANT (UNCHANGED)
    // =========================

    public class FairyEnchantConfiguration
    {
        public int GoldCost { get; set; }
        public List<FairySystemItem> Items { get; set; }
    }

    // =========================
    // ROOT CONFIG
    // =========================

    public class FairySystemConfiguration
    {
        public FairyUpgradeConfiguration Upgrade { get; set; }
        public FairyEnchantConfiguration Enchant { get; set; }
    }
}