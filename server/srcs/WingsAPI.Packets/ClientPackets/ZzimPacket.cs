// WingsEmu
// 
// Developed by NosWings Team

namespace WingsEmu.Packets.ClientPackets
{
    /// <summary>
    /// NosMall wishlist packet - sent when player adds/removes items from wishlist
    /// Format: zzim {type} {vnum}
    /// Type 1 = Add to wishlist
    /// Type 2 = Remove from wishlist (assumed)
    /// </summary>
    [PacketHeader("zzim")]
    public class ZzimPacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public int ItemVnum { get; set; }
    }
}
