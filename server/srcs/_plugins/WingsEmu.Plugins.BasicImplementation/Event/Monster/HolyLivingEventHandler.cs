using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Monster.Event;

namespace WingsEmu.Plugins.BasicImplementations.Event.Monster;

public class HolyLivingEventHandler : IAsyncEventProcessor<HolyLivingEvent>
{
    private readonly IBuffFactory _buffFactory;

    public HolyLivingEventHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public async Task HandleAsync(HolyLivingEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntity = e.MonsterEntity;
        if (monsterEntity == null)
        {
            return;
        }

        IReadOnlyList<IBattleEntity> sessions = monsterEntity.MapInstance.GetBattleEntitiesInRange(monsterEntity.Position, 5);
        foreach (IBattleEntity session in sessions.Where(s => !s.IsEnemyWith(monsterEntity.Spawner) && s.IsPlayer() && !s.BuffComponent.HasBuff((int)BuffVnums.HOLY_TOTEM_BUFF)))
        {
            await session.AddBuffAsync(_buffFactory.CreateBuff((int)BuffVnums.HOLY_TOTEM_BUFF, session));
        }
    }
}