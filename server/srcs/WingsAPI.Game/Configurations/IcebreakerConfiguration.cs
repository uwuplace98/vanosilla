using System.Collections.Generic;

namespace WingsEmu.Game.Configurations
{
    public class IcebreakerConfiguration
    {
        public int LobbyDurationSeconds { get; set; } = 60;
        public int RoundDurationSeconds { get; set; } = 600;
        public int JoinGoldCost { get; set; } = 500;
        public int WinGoldReward { get; set; } = 10000;
        public int WinReputationReward { get; set; } = 1000;
        public List<IcebreakerChannelConfiguration> Channels { get; set; } = new();
    }

    public class IcebreakerChannelConfiguration
    {
        public int ChannelId { get; set; }
        public List<IcebreakerInstanceConfiguration> Instances { get; set; } = new();
    }

    public class IcebreakerInstanceConfiguration
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int MapId { get; set; } // The map Vnum for this bracket
        public short PortalX { get; set; }
        public short PortalY { get; set; }
    }
}
