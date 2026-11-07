using WingsEmu.Game.Inventory;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event
{
    public class FairyEnchantEvent : PlayerEvent
    {
        public InventoryItem Item { get; init; }
    }
}
