// WingsEmu
// 
// Developed by NosWings Team

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("mall")]
    public class MallPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public string Arg1 { get; set; }

        [PacketIndex(2)]
        public string Arg2 { get; set; }

        [PacketIndex(3)]
        public string Arg3 { get; set; }

        #endregion
    }
}