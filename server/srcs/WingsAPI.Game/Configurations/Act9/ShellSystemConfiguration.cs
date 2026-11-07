using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act9
{
    public class ShellSystemItem
    {
        public int Vnum { get; set; }
        public int Amount { get; set; }
    }

    public class ShellEnchantConfiguration
    {
        public int GoldCost { get; set; }
        public List<ShellSystemItem> Items { get; set; }
    }

    public class ShellSystemConfiguration
    {
        public ShellEnchantConfiguration Enchant { get; set; }
    }
}
