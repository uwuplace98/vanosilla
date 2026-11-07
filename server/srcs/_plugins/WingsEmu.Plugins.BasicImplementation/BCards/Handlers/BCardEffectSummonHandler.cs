using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardEffectSummonHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.EffectSummon;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.EffectSummon.BlockNegativeEffect:
            {
                sender.BCardDataComponent.BlockBadBuff = 0;
            }
                break;
        }
    }
}