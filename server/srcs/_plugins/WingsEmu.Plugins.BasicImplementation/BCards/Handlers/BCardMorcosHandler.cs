using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMorcosHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IRandomGenerator _randomGenerator;

    public BCardMorcosHandler(GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory, INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory, IRandomGenerator randomGenerator)
    {
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffFactory = buffFactory;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _randomGenerator = randomGenerator;
    }

    public BCardType HandledType => BCardType.LordMorcos;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.LordMorcos.AddAdditionalHp:
                if (sender is not IPlayerEntity playerEntity1)
                {
                    return;
                }
                
                if (playerEntity1.LastBCardHealth.AddSeconds(3) > DateTime.UtcNow)
                {
                    return;
                }
                
                IMateEntity sunWolf1 = playerEntity1.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);
                
                if (sunWolf1 == null || !sunWolf1.IsAlive())
                {
                    return;
                }

                int maxHp = (int)Math.Round(sunWolf1.Hp * 0.20);

                if (playerEntity1.AdditionalHp >= (int)Math.Round(playerEntity1.MaxHp * 0.5))
                {
                    return;
                }

                if (maxHp > (int)Math.Round(playerEntity1.MaxHp * 0.5))
                {
                    maxHp = (int)Math.Round(playerEntity1.MaxHp * 0.5);
                }
                
                playerEntity1.LastBCardHealth = DateTime.UtcNow;
                playerEntity1.AddAdditionalSnack(maxHp, maxHp, true, maxHp);
                break;
            
            case (byte)AdditionalTypes.LordMorcos.ReswpanDeadWolfHalfHp:
            {
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }
                
                IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);
                
                if (sunWolf != null && sunWolf.IsAlive())
                {
                    return;
                }
                
                playerEntity.SpawnSunchaserWolf(_npcMonsterManager, _mateEntityFactory, true).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;

            case (byte)AdditionalTypes.LordMorcos.NaturalHealingBuff:
            {
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }

                if (_randomGenerator.RandomNumber(101) > firstDataValue)
                {
                    return;
                }

                Buff buffToAdd = _buffFactory.CreateBuff(secondDataValue, sender);
                IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

                sunWolf?.AddBuffAsync(buffToAdd).ConfigureAwait(false).GetAwaiter().GetResult();
                playerEntity.AddBuffAsync(buffToAdd).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;

            case (byte)AdditionalTypes.LordMorcos.InflictDamageAfter:

                IEnumerable<IBattleEntity> toDamage = sender.MapInstance.GetNonMonsterBattleEntities(x => !sender.Position.IsInAoeZone(x.Position, firstDataValue)).Take(50);

                foreach (IBattleEntity entity in toDamage)
                {
                    if (!entity.IsAlive())
                    {
                        continue;
                    }

                    int damage = (int)(entity.MaxHp * (secondDataValue * 0.01));

                    if (sender.ShouldSaveDefender(entity, damage, _gameRevivalConfiguration, _buffFactory).ConfigureAwait(false).GetAwaiter().GetResult())
                    {
                        continue;
                    }

                    if (entity.Hp - damage <= 0)
                    {
                        entity.Hp = 0;
                        entity.EmitEvent(new GenerateEntityDeathEvent
                        {
                            Entity = entity,
                            Attacker = sender
                        });

                        sender.BroadcastCleanSuPacket(entity, damage);
                        continue;
                    }

                    entity.Hp -= damage;

                    switch (entity)
                    {
                        case IPlayerEntity character:
                            character.LastDefence = DateTime.UtcNow;
                            character.Session.RefreshStat();

                            if (character.IsSitting)
                            {
                                character.Session.RestAsync(force: true);
                            }

                            break;
                        case IMateEntity mate:
                            mate.LastDefence = DateTime.UtcNow;
                            mate.Owner.Session.SendMateLife(mate);

                            if (mate.IsSitting)
                            {
                                mate.Owner.Session.EmitEvent(new MateRestEvent
                                {
                                    MateEntity = mate,
                                    Force = true
                                });
                            }

                            break;
                    }

                    sender.BroadcastCleanSuPacket(entity, damage);
                }

                break;
        }
    }
}