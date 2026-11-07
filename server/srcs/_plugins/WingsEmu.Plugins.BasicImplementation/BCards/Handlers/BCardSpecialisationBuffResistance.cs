// WingsEmu
// 
// Developed by NosWings Team

using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialisationBuffResistance : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardSpecialisationBuffResistance(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public BCardType HandledType => BCardType.SpecialisationBuffResistance;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        int randomNumber = _randomGenerator.RandomNumber();

        switch (subType)
        {
            case (byte)AdditionalTypes.SpecialisationBuffResistance.RemoveBadEffects:
            {
                if (randomNumber < firstData)
                {
                    target.RemoveNegativeBuffs(secondData).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            }

            case (byte)AdditionalTypes.SpecialisationBuffResistance.RemoveGoodEffects:
            {
                if (randomNumber < firstData)
                {
                    target.RemovePositiveBuffs(secondData).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            }
        }
    }
}