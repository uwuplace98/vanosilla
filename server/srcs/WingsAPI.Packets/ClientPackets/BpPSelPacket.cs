using WingsEmu.Packets;
using WingsEmu.Packets.ClientPackets;
using WingsAPI.Packets.Enums.BattlePass;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("bp_psel")]
    public class BpPSelPacket : ClientPacket
    {
        [PacketIndex(0)]
        public BattlePassItemType Type { get; set; }

        [PacketIndex(1)]
        public long BearingId { get; set; }
    }
}
