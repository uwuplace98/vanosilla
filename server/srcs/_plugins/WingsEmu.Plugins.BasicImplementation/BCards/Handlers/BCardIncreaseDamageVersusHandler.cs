using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardIncreaseDamageVersusHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardIncreaseDamageVersusHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.IncreaseDamageVersus;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        int firstData = bCard.FirstDataValue(sender.Level);
        int secondData = bCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.IncreaseDamageVersus.HolyHealTargetHPRefill:

                if (target is IMateEntity)
                {
                    return;
                }

                if (sender is not IPlayerEntity playerSender)
                {
                    return;
                }

                if (target.GetHpPercentage() >= firstData)
                {
                    return;
                }

                playerSender.UpdateEnergyBar(secondData).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}