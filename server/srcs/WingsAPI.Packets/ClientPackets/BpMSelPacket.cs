using WingsEmu.Packets;
using WingsEmu.Packets.ClientPackets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("bp_msel")]
    public class BpMSelPacket : ClientPacket
    {
        [PacketIndex(0)]
        public long QuestId { get; set; }
    }
}
