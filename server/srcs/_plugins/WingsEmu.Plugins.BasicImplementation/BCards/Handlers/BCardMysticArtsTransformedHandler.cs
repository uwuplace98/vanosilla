using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMysticArtsTransformedHandler  : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    
    public BCardMysticArtsTransformedHandler (IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.MysticArtsTransformed;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        
        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.MysticArtsTransformed.CreateFullMoonBoundWhenEnemyBoundByMoonlight when target.BuffComponent.HasBuff((short)BuffVnums.BOUND_BY_MOONLIGHT):
                target.RemoveBuffAsync((short)BuffVnums.BOUND_BY_MOONLIGHT).ConfigureAwait(false).GetAwaiter().GetResult();
                target.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.BOUND_BY_FULL_MOONLIGHT, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}