using WingsEmu.Packets;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("bpt")]
    public class BptPacket : ClientPacket
    {
        [PacketIndex(0)]
        public long MinutesUntilSeasonEnd { get; set; }
    }
}
