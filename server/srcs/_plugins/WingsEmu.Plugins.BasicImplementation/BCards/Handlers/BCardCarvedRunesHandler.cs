// WingsEmu
// 
// Developed by NosWings Team

using PhoenixLib.Events;
using System;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Characters.Events;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCarvedRunesHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IBuffFactory _buffFactory;

    public BCardCarvedRunesHandler(IRandomGenerator randomGenerator, IMonsterEntityFactory monsterEntityFactory, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _monsterEntityFactory = monsterEntityFactory;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.CarvedRunes;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SuPacketHitMode hitMode = ctx.HitMode;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.CarvedRunes.ApocalypsePowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    BCardDTO bCard = buff.BCards.FirstOrDefault(s => s.CardId == buff.CardId);
                    int apocalypseDamage = sender.Level * 18;
                    if (target.Hp - apocalypseDamage <= 0)
                    {
                        if (target.Hp != 1)
                        {
                            target.BroadcastDamage(target.Hp - 1);
                        }

                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(apocalypseDamage);
                        target.Hp -= apocalypseDamage;
                    }

                    sender.BroadCastNoEntitySuPacket(target, apocalypseDamage, hitMode, bCard?.BCardLevel < 5 ? EffectType.ApocalypsePower : EffectType.RedMageUltimate);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes.ReflectionPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes.WolfPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    BCardDTO bCard = buff.BCards.FirstOrDefault(s => s.CardId == buff.CardId);
                    int wolfDamage = sender.Level * 15;
                    if (target.Hp - wolfDamage <= 0)
                    {
                        if (target.Hp != 1)
                        {
                            target.BroadcastDamage(target.Hp - 1);
                        }

                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(wolfDamage);
                        target.Hp -= wolfDamage;
                    }

                    sender.BroadCastNoEntitySuPacket(target, wolfDamage, hitMode, bCard?.BCardLevel < 5 ? EffectType.WolfPower : EffectType.WolfPowerEvolved);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes.OnPvPEnemyPushOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    if (!target.IsAlive())
                    {
                        return;
                    }

                    short sX = sender.PositionX;
                    short sY = sender.PositionY;
                    short tX = target.PositionX;
                    short tY = target.PositionY;

                    switch (target)
                    {
                        case INpcEntity { CanAttack: false }:
                            return;
                        case IMonsterEntity { CanBePushed: false }:
                            return;
                    }

                    (int pushResistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                    if (pushResistance != 0 && _randomGenerator.RandomNumber() <= pushResistance)
                    {
                        return;
                    }

                    sX = sender.PositionX;
                    sY = sender.PositionY;
                    tX = target.PositionX;
                    tY = target.PositionY;


                    int distance = sender.GetDistance(target);
                    if (distance <= 0)
                    {
                        distance = 1;
                    }

                    int d = distance + 4;

                    short maxX = (short)(sX - d * (sX - tX) / distance);
                    short maxY = (short)(sY - d * (sY - tY) / distance);

                    int dx = Math.Abs(maxX - tX);
                    int sx = tX < maxX ? 1 : -1;
                    int dy = -Math.Abs(maxY - tY);
                    int sy = tY < maxY ? 1 : -1;
                    int err = dx + dy;

                    short lastX = tX;
                    short lastY = tY;

                    while (true)
                    {
                        if (sender.MapInstance.IsBlockedZone(tX, tY))
                        {
                            tX = lastX;
                            tY = lastY;
                            break;
                        }

                        lastX = tX;
                        lastY = tY;

                        if (tX == maxX && tY == maxY)
                        {
                            break;
                        }

                        int e2 = 2 * err;
                        if (e2 >= dy)
                        {
                            err += dy;
                            tX += (short)sx;
                        }

                        if (e2 > dx)
                        {
                            continue;
                        }

                        err += dx;
                        tY += (short)sy;
                    }

                    var newPosition = new Position(tX, tY);
                    switch (target)
                    {
                        case IPlayerEntity playerEntity:
                            if (playerEntity.IsSitting)
                            {
                                playerEntity.Session.RestAsync(force: true).ConfigureAwait(false).GetAwaiter().GetResult();
                            }

                            break;
                        case IMateEntity mate:

                            if (mate.IsSitting)
                            {
                                mate.Owner.Session.EmitEvent(new MateRestEvent
                                {
                                    MateEntity = mate,
                                    Rest = false,
                                    Force = true
                                });
                            }
                            break;
                    }

                    sender.MapInstance?.Broadcast(target.GeneratePushPacket(tX, tY, 4));
                    target.ChangePosition(newPosition);
                    target.BroadcastEffectInRange(EffectType.PushSmoke);
                    target.MapInstance.Broadcast(target.GenerateSelfEffect(EffectType.PushBackPower));
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes.ExplosionPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    int explosionDamage = sender.Level * 17;
                    if (target.Hp - explosionDamage <= 0)
                    {
                        if (target.Hp != 1)
                        {
                            target.BroadcastDamage(target.Hp - 1);
                        }

                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(explosionDamage);
                        target.Hp -= explosionDamage;
                    }

                    sender.BroadCastNoEntitySuPacket(target, explosionDamage, hitMode, EffectType.Explosion);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
        }
    }
    
    public bool IsSucceededChance(int chance) => _randomGenerator.RandomNumber() <= chance;
}