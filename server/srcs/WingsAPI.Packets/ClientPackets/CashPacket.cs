using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("cash")]
    public class CashPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int Type { get; set; }

        [PacketIndex(1)]
        public int AccountID { get; set; }

        [PacketIndex(3)]
        public int Coins { get; set; }

        [PacketIndex(4)]
        public int Unknown { get; set; }
        #endregion
    }
}
