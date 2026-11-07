// WingsEmu
// 
// Developed by NosWings Team

using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System;
using System.Linq;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDragonSkillsHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IRandomGenerator _random;
    private readonly ISkillsManager _skillManager;

    public BCardDragonSkillsHandler(IRandomGenerator random, IAsyncEventPipeline eventPipeline, ISkillsManager skillManager)
    {
        _random = random;
        _eventPipeline = eventPipeline;
        _skillManager = skillManager;
    }

    public BCardType HandledType => BCardType.DragonSkills;

    public async void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skillInfo = ctx.Skill;
        int damageDealt = ctx.DamageDealt;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {

            case (byte)AdditionalTypes.DragonSkills.ChangeIntoDragon:
                {
                    if (sender is not IPlayerEntity playerEntity)
                    {
                        return;
                    }

                    playerEntity.Morph = (byte)MorphType.DraconicFistTransformed;
                    playerEntity.Session.SendCModePacket();
                }
                break;

            case (byte)AdditionalTypes.DragonSkills.CooldownResetChance:
            case (byte)AdditionalTypes.DragonSkills.CooldownResetChanceNegated:
                {
                    if (!sender.IsSucceededChance(firstData))
                    {
                        return;
                    }

                    if (target is not IPlayerEntity playerEntity)
                    {
                        return;
                    }

                    if (skillInfo is null)
                    {
                        return;
                    }

                    if (skillInfo.CastId is 0)
                    {
                        return;
                    }

                    SkillInfo skillInfo1 = _skillManager.GetSkill(secondData).GetInfo();
                    playerEntity.ClearSkillCooldownsById(skillInfo1.CastId);
                    IBattleEntitySkill skill = playerEntity.Skills.FirstOrDefault(s => s.Skill.CastId == skillInfo1.CastId);
                    if (skill != null)
                    {
                        skill.LastUse = DateTime.MinValue;
                    }

                    playerEntity.Session.SendSkillCooldownReset(skillInfo1.CastId);
                }
                break;

            case (byte)AdditionalTypes.DragonSkills.MagicArrowChance:
                {
                    if (damageDealt is 0)
                    {
                        return;
                    }

                    if (skillInfo.AttackType != AttackType.Ranged && skillInfo.AttackType != AttackType.Magical)
                    {
                        return;
                    }

                    if (!sender.IsSucceededChance(firstData))
                    {
                        return;
                    }

                    int magicArrowDamage = (int)(damageDealt * 0.15);
                    await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                    {
                        Damaged = target,
                        Damager = sender,
                        Damage = magicArrowDamage,
                        CanKill = false,
                        SkillInfo = skillInfo
                    });
                    sender.BroadcastZephyrPacket(target, skillInfo, magicArrowDamage);
                }
                break;
        }
    }
}