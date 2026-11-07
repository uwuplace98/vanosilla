using System;
using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBlasterHeatHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardBlasterHeatHandler(IRandomGenerator randomGenerator)
    {
        _randomGenerator = randomGenerator;
    }

    public BCardType HandledType => BCardType.BlasterHeat;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int damage = ctx.DamageDealt;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.BlasterHeat.PullEnemiesWithHighHeating:
                {
                    if (!sender.BuffComponent.HasBuff((short)BuffVnums.HIGH_HEATING))
                    {
                        return;
                    }

                    IEnumerable<IBattleEntity> toPull = target.GetEnemiesInRange(sender, (short)firstData);

                    if (!target.IsAlive())
                    {
                        return;
                    }

                    switch (target)
                    {
                        case INpcEntity { CanAttack: false }:
                            return;
                        case IMonsterEntity { CanBePushed: false }:
                            return;
                    }

                    (int resistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                    if (resistance != 0 && _randomGenerator.RandomNumber() <= resistance)
                    {
                        return;
                    }

                    short tX = target.PositionX;
                    short tY = target.PositionY;

                    foreach (IBattleEntity entity in toPull)
                    {
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

                        sender.MapInstance?.Broadcast(entity.GeneratePushPacket(tX, tY, 4));
                        entity.BroadcastEffectInRange(EffectType.PushSmoke);
                        entity.ChangePosition(newPosition);
                    }
                }
                break;
            case (byte)AdditionalTypes.BlasterHeat.PullEnemiesFromSpacesAroundTarget:
                {
                    IEnumerable<IBattleEntity> toPull = target.GetEnemiesInRange(sender, (short)firstData);

                    if (!target.IsAlive())
                    {
                        return;
                    }

                    switch (target)
                    {
                        case INpcEntity { CanAttack: false }:
                            return;
                        case IMonsterEntity { CanBePushed: false }:
                            return;
                    }

                    (int resistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                    if (resistance != 0 && _randomGenerator.RandomNumber() <= resistance)
                    {
                        return;
                    }

                    short tX = target.PositionX;
                    short tY = target.PositionY;

                    foreach (IBattleEntity entity in toPull)
                    {
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

                        sender.MapInstance?.Broadcast(entity.GeneratePushPacket(tX, tY, 4));
                        entity.BroadcastEffectInRange(EffectType.PushSmoke);
                        entity.ChangePosition(newPosition);
                    }
                }
                break;
            case (byte)AdditionalTypes.BlasterHeat.ReduceOpponentHpWithHighHeatingEffect:
                {
                    if (target is null)
                    {
                        return;
                    }

                    if (sender.BuffComponent.HasBuff((short)BuffVnums.HIGH_HEATING) || sender.BuffComponent.HasBuff((short)BuffVnums.ULTRA_HIGH_HEATING))
                    {
                        int hpToDecrease = (int)(damage * (firstData * 0.01));
                        target.EmitEventAsync(new BattleEntityHealEvent
                        {
                            Entity = target,
                            HpHeal = -hpToDecrease
                        });
                        target.BroadcastDamage(hpToDecrease);
                    }
                }
                break;
        }
    }
}