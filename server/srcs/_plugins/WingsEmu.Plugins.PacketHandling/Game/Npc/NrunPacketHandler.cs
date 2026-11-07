using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class NrunPacketHandler : GenericGamePacketHandlerBase<NRunPacket>
{
    private readonly IItemUsageManager _itemUsageManager;

    public NrunPacketHandler(IItemUsageManager itemUsageManager) => _itemUsageManager = itemUsageManager;

    protected override async Task HandlePacketAsync(IClientSession session, NRunPacket packet)
    {

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

        session.PlayerEntity.LastNRunId = packet.NpcId;
        _itemUsageManager.SetLastItemUsed(session.PlayerEntity.Id, 0);
        if (session.PlayerEntity.Hp > 0 && session.HasCurrentMapInstance)
        {
            await session.EmitEventAsync(new NpcDialogEvent
            {
                NpcRunType = packet.Type,
                Argument = packet.Argument,
                VisualType = packet.Value,
                NpcId = packet.NpcId,
                Confirmation = packet.Confirmation
            });
        }
    }
}
