using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBlockHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Block;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.Block.DecreaseFinalCriticalDamagePerHit:
            {
                sender.BCardDataComponent.CriticalDamageDecreased = 0;
            }
                break;
        }
    }
}