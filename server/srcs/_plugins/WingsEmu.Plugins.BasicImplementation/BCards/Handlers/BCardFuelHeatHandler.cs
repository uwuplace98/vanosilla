using PhoenixLib.Events;
using System;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardFuelHeatHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardFuelHeatHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.FuelHeatPoint;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.FuelHeatPoint.ReceiveHeatPoints:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    player.LastEnergyRefill = DateTime.UtcNow;
                    player.UpdateEnergyBar(firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                break;
            case (byte)AdditionalTypes.FuelHeatPoint.LoseHeatPoints:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    player.LastEnergyRefill = DateTime.UtcNow;
                    player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
        }
    }
}