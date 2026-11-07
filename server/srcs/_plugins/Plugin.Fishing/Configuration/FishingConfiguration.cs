using System.Collections.Generic;
using WingsEmu.Game._i18n;

namespace Plugin.Fishing.Configuration
{
    public class FishingConfiguration
    {
        public List<FishingMap> Maps { get; set; } = new();
        public List<FishingPool> Pools { get; set; } = new(); // Spots?
        public List<FishDrop> Drops { get; set; } = new();
    }

    public class FishingMap
    {
        public int MapVnum { get; set; }
        public int RegionId { get; set; } // The ID referenced in POS
    }

    public class FishingPool // POS
    {
        public int RegionId { get; set; }
        public int SubId { get; set; } // ?
        public int X { get; set; }
        public int Y { get; set; }
        public int Direction { get; set; }
    }

    public class FishDrop // ITEM
    {
        public int Id { get; set; } // Referenced by ITEMT?
        public int Vnum { get; set; }
        public int Chance { get; set; } // out of 1000 or 10000?
    }
}
