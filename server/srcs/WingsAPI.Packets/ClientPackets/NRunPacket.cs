// WingsEmu
// 
// Developed by NosWings Team

using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("n_run")]
    public class NRunPacket : ClientPacket //n_run 14 119 2 14849 //n_run 12 83 2 14854

    {
        [PacketIndex(0)]
        public NpcRunType Type { get; set; }

        [PacketIndex(1)]
        public short Argument { get; set; }

        [PacketIndex(2)]
        public VisualType Value { get; set; }

        [PacketIndex(3)]
        public int NpcId { get; set; }

        [PacketIndex(4)]
        public byte? Confirmation { get; set; }
    }
}