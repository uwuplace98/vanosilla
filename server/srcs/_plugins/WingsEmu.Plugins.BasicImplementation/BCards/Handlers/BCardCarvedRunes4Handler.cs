// WingsEmu
// 
// Developed by NosWings Team

using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;


namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCarvedRunes4Handler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IBuffFactory _buffFactory;

    public BCardCarvedRunes4Handler(IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.CarvedRunes4;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.CarvedRunes4.BloodPowerOnDefending:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes4.ConversionPowerOnDefending:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes4.UnyieldingPowerOnDefending:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes4.InstinctOnDefending:
                {
                    if (!IsSucceededChance(firstData))
                    {
                        return;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            case (byte)AdditionalTypes.CarvedRunes4.HealingPowerOnDefending:
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