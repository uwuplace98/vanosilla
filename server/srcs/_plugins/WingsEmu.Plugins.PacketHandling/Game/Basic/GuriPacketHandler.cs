using System.Threading.Tasks;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class GuriPacketHandler : GenericGamePacketHandlerBase<GuriPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GuriPacket packet)
    {
        if (session.PlayerEntity.CheatComponent.IsInvisible)
        {
            return;
        }

        if (packet == null || string.IsNullOrEmpty(packet.OriginalContent))
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

        string[] split = packet.OriginalContent.Split(' ', '^');
        await session.EmitEventAsync(new GuriEvent
        {
            EffectId = packet.Type,
            Data = (int)(split[1][0] == '#' ? packet.Argument ?? 0 : packet.Data ?? 0),
            User = packet.User ?? session.PlayerEntity.Id,
            Value = packet.Value,
            Packet = split
        });
    }
}