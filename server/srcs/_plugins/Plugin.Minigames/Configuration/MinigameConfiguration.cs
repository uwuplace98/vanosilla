using System;

namespace Plugin.Minigames.Configuration
{
    public class MinigameConfiguration
    {
        // Sheep Farm Minigame
        public int SheepFarmMapId { get; set; } = 2009;
        public int SheepMonsterVNum { get; set; } = 9;
        public int SheepMorphId { get; set; } = 1009;
        public int SheepGameDurationMinutes { get; set; } = 5;
        public int SheepSpeed { get; set; } = 5;

        // Meteorite Dodge Minigame
        public int MeteoriteDodgeMapId { get; set; } = 2004;
        public int CircleMonsterVNum { get; set; } = 2018;
        public int CircleEffectId { get; set; } = 4660;
        public int MeteoriteSpeed { get; set; } = 12;
        public int MeteoriteMorphId { get; set; } = 1156;
        public int MeteoriteRounds { get; set; } = 5;
        public int CircleDamageDelayMs { get; set; } = 3000;

        // Rewards
        public int GoldPerRound { get; set; } = 10000;
        public int SurvivalBonusGold { get; set; } = 50000;
        public int FirstPlaceBonus { get; set; } = 100000;
        public int SecondPlaceBonus { get; set; } = 50000;
        public int ThirdPlaceBonus { get; set; } = 25000;
        public int GoldPerSheep { get; set; } = 100;
    }
}
