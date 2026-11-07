// WingsEmu
// 
// Developed by NosWings Team

using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardFearSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public BCardFearSkillHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.FearSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        
        if (target is not IPlayerEntity character)
        {
            return;
        }

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.FearSkill.MoveAgainstWill:
                character.Session.SendOppositeMove(true);
                break;
            case (byte)AdditionalTypes.FearSkill.TimesUsed:
                int buffVnum = secondData;
                switch (firstData)
                {
                    case 1:
                        if (character.ScoutStateType != ScoutStateType.None)
                        {
                            return;
                        }

                        Buff firstBuff = _buffFactory.CreateBuff(buffVnum, character);
                        character.AddBuffAsync(firstBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case 2:
                        if (character.BuffComponent.HasBuff(buffVnum))
                        {
                            return;
                        }

                        if (character.ScoutStateType != ScoutStateType.FirstState)
                        {
                            return;
                        }

                        if (!character.BuffComponent.HasBuff(buffVnum - 1))
                        {
                            return;
                        }

                        Buff secondBuff = _buffFactory.CreateBuff(buffVnum, character);
                        character.AddBuffAsync(secondBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    default:
                        return;
                }

                break;
        }
    }
}