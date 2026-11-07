// WingsEmu
// 
// Developed by NosWings Team

using System.Linq;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;


namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCarvedRunes2Handler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IBuffFactory _buffFactory;

    public BCardCarvedRunes2Handler(IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.CarvedRunes2;

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
            case (byte)AdditionalTypes.CarvedRunes2.AgilityPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes2.LightningPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    BCardDTO bCard = buff.BCards.FirstOrDefault(s => s.CardId == buff.CardId);
                    
                    int lightningDamage = sender.Level * 18;
                    
                    if (target.Hp - lightningDamage <= 0)
                    {
                        if (target.Hp != 1)
                        {
                            target.BroadcastDamage(target.Hp - 1);
                        }

                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(lightningDamage);
                        target.Hp -= lightningDamage;
                    }

                    sender.BroadCastNoEntitySuPacket(target, lightningDamage, hitMode, bCard?.BCardLevel < 5 ? EffectType.LightningPower : EffectType.LightningPowerEvolved);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes2.CursePowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes2.BearPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    int bearDamage = sender.Level * 23;
                    if (target.Hp - bearDamage <= 0)
                    {
                        if (target.Hp != 1)
                        {
                            target.BroadcastDamage(target.Hp - 1);
                        }

                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(bearDamage);
                        target.Hp -= bearDamage;
                    }

                    //TODO: Find effect Id
                    sender.BroadCastNoEntitySuPacket(target, bearDamage, hitMode, EffectType.Stars);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes2.FrostPowerOnAttack:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
        }
    }
    
    public bool IsSucceededChance(int chance) => _randomGenerator.RandomNumber() <= chance;
}