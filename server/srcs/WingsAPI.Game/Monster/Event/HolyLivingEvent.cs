using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Monster.Event;

public class HolyLivingEvent : IAsyncEvent
{
    public IMonsterEntity MonsterEntity { get; set; }
}