// WingsEmu
// 
// Developed by Ascension Team

using System.Collections.Generic;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Helpers.Damages.Calculation;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Plugins.BasicImplementations.Algorithms;

public class DamageAlgorithm : IDamageAlgorithm
{
    private readonly IBuffFactory _buffFactory;

    private readonly HashSet<int> _specialSkills = new()
    {
        (int)SkillsVnums.GIANT_SWIRL,
        (int)SkillsVnums.FOG_ARROW,
        (int)SkillsVnums.FIRE_MINE,
        (int)SkillsVnums.BOMB
    };

    public DamageAlgorithm(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public DamageAlgorithmResult GenerateDamage(IBattleEntityDump attacker, IBattleEntityDump defender, SkillInfo skill, bool ignore = false)
    {
        int damages = 0;
        HitType hitMode = HitType.Normal;
        bool onyxEffect = false;
        bool zephyrEffect = false;
        
        if (defender == null)
        {
            return new DamageAlgorithmResult(damages, hitMode, false, false, false);
        }

        if (skill != null && !ignore)
        {
            if (skill.TargetAffectedEntities != TargetAffectedEntities.Enemies || _specialSkills.Contains(skill.Vnum))
            {
                return new DamageAlgorithmResult(damages, hitMode, false, false, false);
            }
        }

        CalculationBasicStatistics basicCalculation = attacker.CalculateBasicStatistics(defender, skill);

        IBattleEntity targetEntity = attacker.MapInstance.GetBattleEntity(defender.Type, defender.Id);
        
        if (attacker.IsMiss(defender, basicCalculation))
        {
            hitMode = HitType.Miss;
            return new DamageAlgorithmResult(damages, hitMode, false, false, false);
        }

        CalculationDefense defense = attacker.CalculationDefense(defender);
        CalculationPhysicalDamage physicalDamage = attacker.CalculatePhysicalDamage(defender, skill);
        CalculationElementDamage elementDamage = attacker.CalculateElementDamage(defender, skill);
        CalculationResult damageResult = attacker.DamageResult(defender, basicCalculation, defense, physicalDamage, elementDamage, skill);

        if (damageResult.IsCritical)
        {
            hitMode = HitType.Critical;
        }

        damages = damageResult.Damage;

        (int firstData, int secondData, int count) onyxBuff = attacker.GetBCardInformation(BCardType.StealBuff, (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon);
        if (attacker.IsSucceededChance(onyxBuff.firstData))
        {
            onyxEffect = true;
        }
        
        if (skill?.AttackType is AttackType.Magical or AttackType.Ranged)
        {
            int magicArrowBuff = attacker.GetBCardInformation(BCardType.DragonSkills, (byte)AdditionalTypes.DragonSkills.MagicArrowChance).firstData;
            if (attacker.IsSucceededChance(magicArrowBuff))
            {
                zephyrEffect = true;
            }
        }

        if (defender.GetBCardInformation(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower).count == 0)
        {
            return new DamageAlgorithmResult(damages, hitMode, onyxEffect, damageResult.IsSoftDamage, zephyrEffect);
        }
        
        if (targetEntity == null)
        {
            return new DamageAlgorithmResult(damages, hitMode, onyxEffect, damageResult.IsSoftDamage, zephyrEffect);
        }
        
        if (defender.GetBCardInformation(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower).count == 0)
        {
            return new DamageAlgorithmResult(damages, hitMode, onyxEffect, damageResult.IsSoftDamage, zephyrEffect);
        }

        if (targetEntity.ChargeComponent.GetCharge() != 0)
        {
            return new DamageAlgorithmResult(0, HitType.Miss, false, false, false);
        }
        
        targetEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.CHARGE, targetEntity)).ConfigureAwait(false).GetAwaiter().GetResult();
        return new DamageAlgorithmResult(0, HitType.Miss, false, false, false);
    }
}