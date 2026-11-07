using ProtoBuf;

namespace WingsAPI.Data.NosMall
{
    [ProtoContract]
    public class NosMallItemDto
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public long Price { get; set; }

        [ProtoMember(4)]
        public string Description { get; set; }

        [ProtoMember(5)]
        public int ItemVnum { get; set; }

        [ProtoMember(6)]
        public int Amount { get; set; }

        [ProtoMember(7)]
        public bool IsLimited { get; set; }

        [ProtoMember(8)]
        public bool IsHit { get; set; }

        [ProtoMember(9)]
        public bool IsNew { get; set; }

        [ProtoMember(10)]
        public bool IsSelling { get; set; }

        [ProtoMember(11)]
        public int TabId { get; set; }

        [ProtoMember(12)]
        public string Title2 { get; set; }

        [ProtoMember(13)]
        public byte DurationType { get; set; }

        [ProtoMember(14)]
        public int DurationValue { get; set; }
    }
}
