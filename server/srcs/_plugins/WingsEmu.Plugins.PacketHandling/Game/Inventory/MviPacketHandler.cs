using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class MviPacketHandler : GenericGamePacketHandlerBase<MviPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, MviPacket mviPacket)
    {
        if (mviPacket.Amount <= 0)
        {
            return;
        }

        if (mviPacket.Slot == mviPacket.DestinationSlot)
        {
            return;
        }

        if (mviPacket.InventoryType == InventoryType.EquippedItems || mviPacket.InventoryType == InventoryType.Miniland)
        {
            return;
        }

        // check if the destination slot is out of range
        if (mviPacket.DestinationSlot > session.PlayerEntity.GetInventorySlots(inventoryType: mviPacket.InventoryType))
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to use item while being in exchange");
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to use item while having shop open");
            return;
        }

        await session.EmitEventAsync(new InventoryMoveItemEvent(mviPacket.InventoryType, mviPacket.Slot, mviPacket.Amount, mviPacket.DestinationSlot, mviPacket.InventoryType));
    }
}
