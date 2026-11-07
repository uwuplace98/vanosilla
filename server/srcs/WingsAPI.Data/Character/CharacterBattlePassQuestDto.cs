using ProtoBuf;

namespace WingsAPI.Data.Character
{
    [ProtoContract]
    public class CharacterBattlePassQuestDto
    {
        [ProtoMember(1)]
        public long QuestId { get; set; }

        [ProtoMember(2)]
        public long Progress { get; set; }

        [ProtoMember(3)]
        public bool IsFinished { get; set; }
    }
}
