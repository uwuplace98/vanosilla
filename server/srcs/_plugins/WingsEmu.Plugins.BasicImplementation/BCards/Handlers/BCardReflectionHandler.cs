using System;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardReflectionHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Reflection;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        int damage = ctx.DamageDealt;

        switch (subType)
        {
            case (byte)AdditionalTypes.Reflection.HPIncreased:
                {
                    if (!sender.IsAlive())
                    {
                        return;
                    }

                    int hpToIncrease = (int)(damage * (firstData * 0.01));
                    sender.EmitEventAsync(new BattleEntityHealEvent
                    {
                        Entity = sender,
                        HpHeal = hpToIncrease
                    });
                }
                break;
            case (byte)AdditionalTypes.Reflection.MPIncreased:
                {
                    if (!sender.IsAlive())
                    {
                        return;
                    }

                    if (!sender.BCardComponent.HasBCard(BCardType.Reflection, (byte)AdditionalTypes.Reflection.MPIncreased))
                    {
                        return;
                    }

                    int mpToIncrease = (int)(damage * (firstData * 0.01));
                    if (sender.Mp + mpToIncrease < sender.MaxMp)
                    {
                        sender.Mp += mpToIncrease;
                    }
                    else
                    {
                        sender.Mp = sender.MaxMp;
                    }
                }
                break;
            case (byte)AdditionalTypes.Reflection.EnemyHPDecreased:
                {
                    if (!target.IsAlive())
                    {
                        return;
                    }

                    int hpToDecrease = (int)(damage * (firstData * 0.01));
                    target.EmitEventAsync(new BattleEntityHealEvent
                    {
                        Entity = target,
                        HpHeal = -hpToDecrease
                    });
                    target.BroadcastDamage(hpToDecrease);
                }
                break;
            case (byte)AdditionalTypes.Reflection.ChanceMpLost:
                {
                    if (!target.IsSucceededChance(firstData))
                    {
                        return;
                    }

                    int loss = (int)(target.Mp * (secondData * 0.01));
                    target.Mp = target.Mp - loss <= 0 ? 0 : target.Mp - loss;
                }
                break;
        }
    }
}