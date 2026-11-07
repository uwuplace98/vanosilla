using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardAbsorptionHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Absorption;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.Absorption.AbsordMaxHPAsDamage:
            {
                sender.BCardDataComponent.AbsorptionDamage = (int)(sender.MaxHp * firstData * 0.01);
            }
                break;
        }
    }
}