using ProtoBuf;
using WingsAPI.Data;

namespace WingsEmu.DTOs.Items
{
    [ProtoContract]
    public class FairyEnchantmentDTO
    {
        [ProtoMember(1)]
        public byte EffectId { get; set; }

        [ProtoMember(2)]
        public int EffectValue { get; set; }

        [ProtoMember(3)]
        public byte EffectLevel { get; set; }
    }
}
