using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Act9.Fairies;

public class UpgradeFairiesEvent : PlayerEvent
{
    public UpgradeFairiesEvent(InventoryItem fairy)
    {
        Fairy = fairy;
    }
    
    public InventoryItem Fairy { get; }
}