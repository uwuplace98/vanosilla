using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Networking;
using WingsEmu.Packets;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    [PacketHeader("bp_close")]
    public class BpClosePacket : ClientPacket
    {
    }

    public class BpClosePacketHandler : GenericGamePacketHandlerBase<BpClosePacket>
    {
        protected override async Task HandlePacketAsync(IClientSession session, BpClosePacket packet)
        {
            // Just handling the packet to avoid UNRESOLVED_PACKET warning.
            // Logic can be added here if needed, e.g., tracking UI state.
            await Task.CompletedTask;
        }
    }
}
