// WingsEmu
// 
// Developed by NosWings Team

using PhoenixLib.Events;
using System;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardStealBuffHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IAsyncEventPipeline _eventPipeline;

    public BCardStealBuffHandler(IRandomGenerator randomGenerator, IMonsterEntityFactory monsterEntityFactory, IAsyncEventPipeline eventPipeline)
    {
        _randomGenerator = randomGenerator;
        _monsterEntityFactory = monsterEntityFactory;
        _eventPipeline = eventPipeline;
    }

    public BCardType HandledType => BCardType.StealBuff;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skill = ctx.Skill;
        SuPacketHitMode hitMode = ctx.HitMode;
        int damageDealt = ctx.DamageDealt;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon:

                if (skill is not null && skill.CastId is 0)
                {
                    return;
                }

                if (damageDealt is 0)
                {
                    return;
                }

                if (!sender.IsSucceededChance(firstData))
                {
                    return;
                }

                IMonsterEntity onyxMonsterEntity = null;
                short x = sender.PositionX;
                short y = sender.PositionY;

                x += (short)_randomGenerator.RandomNumber(-3, 3);
                y += (short)_randomGenerator.RandomNumber(-3, 3);

                if (sender.MapInstance.IsBlockedZone(x, y))
                {
                    x = sender.PositionX;
                    y = sender.PositionY;
                }

                onyxMonsterEntity = _monsterEntityFactory.CreateMonster((int)MonsterVnum.ONYX_MONSTER, sender.MapInstance, new MonsterEntityBuilder
                {
                    IsRespawningOnDeath = false,
                    IsWalkingAround = false
                });

                onyxMonsterEntity.EmitEventAsync(new MapJoinMonsterEntityEvent(onyxMonsterEntity, x, y));
                sender.MapInstance.Broadcast(sender.GenerateOnyxGuriPacket(x, y));

                int onyxDamage = damageDealt / 2;
                _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                {
                    Damaged = target,
                    Damager = onyxMonsterEntity,
                    Damage = onyxDamage,
                    CanKill = false,
                    SkillInfo = skill
                });

                onyxMonsterEntity.BroadcastSuPacket(target, skill, onyxDamage, hitMode);
                break;
        }
    }
}