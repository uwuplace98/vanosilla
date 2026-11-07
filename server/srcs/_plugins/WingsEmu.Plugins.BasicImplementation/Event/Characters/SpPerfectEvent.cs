using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Plugins.BasicImplementations.Event.Characters;

public class SpPerfectEvent : PlayerEvent
{
    public SpPerfectEvent(InventoryItem inventoryItem, bool isFastUpgrade = false)
    {
        InventoryItem = inventoryItem;
        IsFastUpgrade = isFastUpgrade;
    }

    public InventoryItem InventoryItem { get; }
    
    public bool IsFastUpgrade { get; }
}