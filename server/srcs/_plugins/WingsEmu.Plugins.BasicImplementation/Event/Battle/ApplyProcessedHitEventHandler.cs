using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Plugins.BasicImplementations.Event.Battle;

public class ApplyProcessedHitEventHandler : IAsyncEventProcessor<ApplyHitEvent>
{
    private readonly IBCardEffectHandlerContainer _bCardHandlerContainer;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISkillUsageManager _skillUsageManager;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly IBuffFactory _buffFactory;

    public ApplyProcessedHitEventHandler(IAsyncEventPipeline eventPipeline, IBCardEffectHandlerContainer bCardHandlerContainer,
        ISkillUsageManager skillUsageManager, IRandomGenerator randomGenerator, IMonsterEntityFactory monsterEntityFactory,
        ISacrificeManager sacrificeManager, IBuffFactory buffFactory)
    {
        _eventPipeline = eventPipeline;
        _bCardHandlerContainer = bCardHandlerContainer;
        _skillUsageManager = skillUsageManager;
        _randomGenerator = randomGenerator;
        _monsterEntityFactory = monsterEntityFactory;
        _sacrificeManager = sacrificeManager;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(ApplyHitEvent e, CancellationToken cancellation)
    {
        HitInformation hit = e.HitInformation;

        IBattleEntity caster = hit.Caster;
        IBattleEntity target = e.Target;
        DamageAlgorithmResult algorithmResult = e.ProcessResults;
        HitType hitType = algorithmResult.HitType;
        int totalDamage = algorithmResult.Damages;

        SkillInfo skill = hit.Skill;

        BCardDTO[] afterAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> bCards) ? bCards.ToArray() : Array.Empty<BCardDTO>();

        if (hit.IsFirst)
        {
            switch (skill.TargetType)
            {
                case TargetType.Self when skill.HitType == TargetHitType.EnemiesInAffectedAoE:
                    caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageFail, isFirst: hit.IsFirst);
                    break;
                case TargetType.NonTarget:
                    caster.BroadcastNonTargetSkill(e.HitInformation.Position, skill);
                    break;
            }
        }

        if (caster.IsPlayer())
        {
            var player = (IPlayerEntity)caster;
            player.SkillComponent.IsSkillInterrupted = false;
            player.SkillComponent.CanBeInterrupted = false;
        }

        SuPacketHitMode hitMode = GetHitMode(skill, hitType, hit.IsFirst);

        if (!caster.IsAlive())
        {
            hitMode = SuPacketHitMode.OutOfRange;
            caster.BroadcastSuPacket(caster, skill, 0, hitMode, isFirst: hit.IsFirst);
            Skip(caster, skill);
            return;
        }

        switch (target)
        {
            case IMonsterEntity monsterEntity:
                monsterEntity.MapInstance.MonsterRefreshTarget(monsterEntity, caster, DateTime.UtcNow, true);
                break;
            case INpcEntity npcEntity:
                npcEntity.MapInstance.NpcRefreshTarget(npcEntity, caster);
                break;
        }

        int monsterSize = target switch
        {
            IMonsterEntity monsterEntity => monsterEntity.CellSize,
            INpcEntity npcEntity => npcEntity.CellSize,
            IMateEntity mateEntity => mateEntity.CellSize,
            _ => 0
        };

        int cellSizeBonus = target switch
        {
            IPlayerEntity => 7,
            _ => 3
        };

        if (skill.Vnum != -1 && skill.CastId != -1 &&
            skill.HitType == TargetHitType.TargetOnly && !caster.Position.IsInRange(target.Position, skill.Range + monsterSize + cellSizeBonus)
            && !skill.BCards.Any(x => x.Type == (short)BCardType.JumpBackPush && x.SubType == (byte)AdditionalTypes.JumpBackPush.JumpBackChance))
        {
            hitMode = SuPacketHitMode.OutOfRange;
            caster.BroadcastSuPacket(target, skill, 0, hitMode, isFirst: hit.IsFirst);
            Skip(caster, skill);
            return;
        }

        if (target.BCardComponent.HasBCard(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.AddDamageToHP))
        {
            (int firstData, int secondData) bCard =
                target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.AddDamageToHP, target.Level);
            if (_randomGenerator.RandomNumber() <= bCard.firstData)
            {
                double toHealPercentage = bCard.secondData * 0.01;
                int toHeal = (int)(totalDamage * toHealPercentage);
                await target.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = target,
                    HpHeal = toHeal
                });

                caster.BroadcastSuPacket(target, skill, 0, GetHitMode(skill, HitType.Miss, hit.IsFirst), isFirst: hit.IsFirst);
                Skip(caster, skill);
                return;
            }
        }

        if (caster.IsPlayer())
        {
            var character = (IPlayerEntity)caster;

            await character.RemoveInvisibility();

            if (character.TriggerAmbush)
            {
                Buff buff = character.BuffComponent.GetBuff((int)BuffVnums.AMBUSH);
                await character.RemoveBuffAsync(false, buff);
                Buff newBuff = _buffFactory.CreateBuff((int)BuffVnums.AMBUSH_RAID, character);
                await character.AddBuffAsync(newBuff);
                character.TriggerAmbush = false;
            }
        }

        switch (hitType)
        {
            case HitType.Miss:
                {
                    if (caster.IsPlayer())
                    {
                        var character = (IPlayerEntity)caster;
                        character.LastSkillCombo = null;
                        character.Session.SendMsCPacket(0);
                        character.Session.RefreshQuicklist();
                        character.CleanComboState();
                    }
                    
                    foreach ((int _, BCardDTO bCard) in target.BCardComponent.GetBuffBCards())
                    {
                        switch (bCard.Type)
                        {
                            case (short)BCardType.KingOfTheBeast when bCard.SubType is (byte)AdditionalTypes.KingOfTheBeast.MissingHPIncreasedOnDodge:
                            case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff:
                            case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
                                _bCardHandlerContainer.Execute(caster, target, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                                break;
                        }
                    }
                    caster.BroadcastSuPacket(target, skill, 0, hitMode, isFirst: hit.IsFirst);
                    Skip(caster, skill);
                    return;
                }
            case HitType.Critical:
            {
                if (target.BCardComponent.HasBCard(BCardType.ReflectDamage,
                        (byte)AdditionalTypes.ReflectDamage.ReflectDamageOnCritical))
                {
                    await ProbabilityReflection(caster, target, algorithmResult, skill, hit, true);
                    Skip(caster, skill);
                    return;
                }
            }
                break;
        }

        if (caster.IsPlayer())
        {
            var character = (IPlayerEntity)caster;
            
            if (algorithmResult.SoftDamageEffect && hit.IsFirst)
            {
                caster.MapInstance.Broadcast(character.GenerateEffectPacket(EffectType.BoostedAttack));
            }

            if (skill.Combos.Any())
            {
                double increaseDamageByComboState = 0;
                ComboState comboState = _skillUsageManager.GetComboState(caster.Id, target.Id);
                increaseDamageByComboState = 0.05 + 0.1 * comboState.Hit;

                if (target.BCardDataComponent.MaxCriticals is null)
                {
                    totalDamage += (int)(totalDamage * increaseDamageByComboState);
                }

                comboState.Hit++;
                ComboDTO combo = skill.Combos.FirstOrDefault(s => s.Hit == comboState.Hit);
                if (combo != null)
                {
                    skill.HitAnimation = combo.Animation;
                    skill.HitEffect = combo.Effect;
                }

                if (skill.Combos.Max(s => s.Hit) <= comboState.Hit)
                {
                    _skillUsageManager.ResetComboState(caster.Id);
                }
            }
        }

        if (caster.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.NeverCauseDamage))
        {
            totalDamage = 0;
        }

        if (target.IsPlayer())
        {
            var character = (IPlayerEntity)target;
            if (character.SkillComponent.CanBeInterrupted && character.IsCastingSkill)
            {
                character.SkillComponent.CanBeInterrupted = false;
                character.SkillComponent.IsSkillInterrupted = true;
            }
        }

        // REFLECTION
        if (IsReflectionNoDamage(target))
        {
            await ReflectDamage(caster, target, algorithmResult, skill, hit);
            Skip(caster, skill);
            return;
        }

        if (IsReflectionWithDamage(target))
        {
            await ReflectDamage(caster, target, algorithmResult, skill, hit, true);
        }

        if (IsReflectionWithProbability(target))
        {
            await ProbabilityReflection(caster, target, algorithmResult, skill, hit);
            Skip(caster, skill);
        }

        if (target.BCardComponent.HasBCard(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage))
        {
            IBattleEntity receiver = _sacrificeManager.GetCaster(target);

            (int firstData, int secondData) transferBCard = target.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage, target.Level);

            if (receiver != null && receiver.IsAlive())
            {
                int damageToDealt = (int)(totalDamage * (transferBCard.firstData * 0.01));
                int damageToDecrease = (int)(totalDamage * (transferBCard.secondData * 0.01));

                if (receiver.Hp - damageToDealt <= 0)
                {
                    receiver.Hp = 1;
                }
                else
                {
                    receiver.Hp -= damageToDealt;
                }

                receiver.BroadcastCleanSuPacket(receiver, damageToDealt, true);
                totalDamage -= damageToDecrease;
            }
        }
        
        // if (skill.BCards.Any(x => x.Type == (short)BCardType.LordHatus && x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf))
        // {
        //     if (caster is not IPlayerEntity playerEntity)
        //     {
        //         return;
        //     }
        //         
        //     IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);
        //
        //     if (sunWolf == null)
        //     {
        //         return;
        //     }
        //
        //     playerEntity.BroadcastEffectGround(EffectType.SunWolf, sunWolf.PositionX, sunWolf.PositionY, true);
        //     playerEntity.Session.SendAtctl(sunWolf, target);
        //     
        //     Console.WriteLine($"TotalDamage: {totalDamage}");
        //         
        //     sunWolf.BroadcastSuPacket(target, skill, totalDamage, hitMode, isFirst: hit.IsFirst);
        //     sunWolf.ChangePosition(new Position(target.PositionX, target.PositionY));
        //     sunWolf.MapInstance.Broadcast(sunWolf.GenerateTeleportPacket(target.PositionX, target.PositionY));
        //     target.BroadcastDamage(totalDamage, DmType.MateDamage);
        //     
        //     await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
        //     {
        //         Damaged = target,
        //         Damager = sunWolf,
        //         Damage = totalDamage,
        //         CanKill = true,
        //         SkillInfo = skill
        //     });
        // }

        await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
        {
            Damaged = target,
            Damager = caster,
            Damage = totalDamage,
            CanKill = true,
            SkillInfo = skill
        });

        caster.BroadcastSuPacket(target, skill, totalDamage, hitMode, isFirst: hit.IsFirst);

        if (hit.IsFirst)
        {
            hit.IsFirst = false;
        }

        if (target.IsAlive())
        {
            foreach (BCardDTO bCard in afterAttackAllTargets)
            {
                _bCardHandlerContainer.Execute(target, caster, bCard, skill);
            }

            foreach ((int _, BCardDTO bCard) in caster.BCardComponent.GetBuffBCards())
            {
                switch (bCard.Type)
                {
                    case (short)BCardType.StealBuff when bCard.SubType is (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon:
                    case (short)BCardType.DragonSkills when bCard.SubType is (byte)AdditionalTypes.DragonSkills.MagicArrowChance:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcLionLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcEagleLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcSnakeLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcBearLoa:
                    case (short)BCardType.Quest when bCard.SubType is (byte)AdditionalTypes.Quest.HealHPByInflictedDamages:
                    case (short)BCardType.SniperAttack when bCard.SubType is (byte)AdditionalTypes.SniperAttack.ChanceCausing:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.HPIncreased:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.MPIncreased:
                    case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)AdditionalTypes.FourthGlacernonFamilyRaid.HPIncreasedDamageGiven:
                    case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileAttackingChance:
                        _bCardHandlerContainer.Execute(target, caster, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                        break;
                }
            }

            foreach ((int _, BCardDTO bCard) in target.BCardComponent.GetBuffBCards())
            {
                switch (bCard.Type)
                {
                    case (short)BCardType.BlasterHeat when bCard.SubType is (byte)AdditionalTypes.BlasterHeat.ReduceOpponentHpWithHighHeatingEffect:
                    case (short)BCardType.VoodooPriest when bCard.SubType is (byte)AdditionalTypes.VoodooPriest.TakeDamageXTimesBuffDisappear:
                    case (short)BCardType.InflictSkill when bCard.SubType is (byte)AdditionalTypes.InflictSkill.OnDefenceResetCooldownOfSkillUsed:
                    case (short)BCardType.LordHatus when bCard.SubType is (byte)AdditionalTypes.LordHatus.RestoreHPByDamageTaken:
                    case (short)BCardType.SecondSPCard when bCard.SubType is (byte)AdditionalTypes.SecondSPCard.HitAttacker:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.EnemyHPDecreased:
                    case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff:
                    case (short)BCardType.RecoveryAndDamagePercent when bCard.SubType is (byte)AdditionalTypes.RecoveryAndDamagePercent.MPRecovered:
                    case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
                        _bCardHandlerContainer.Execute(caster, target, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                        break;
                }
            }
        }

        if (!target.IsAlive() && caster.IsPlayer())
        {
            (caster as IPlayerEntity).Session.SendCancelPacket(CancelType.NotInCombatMode);
        }

        switch (skill.Vnum)
        {
            case (short)SkillsVnums.HOLY_EXPLOSION when target.BuffComponent.HasBuff((short)BuffVnums.ILLUMINATING_POWDER):
                {
                    await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                    {
                        Damaged = target,
                        Damager = caster,
                        Damage = totalDamage,
                        CanKill = true,
                        SkillInfo = skill
                    });

                    caster.BroadcastSuPacket(target, skill, totalDamage, hitMode, true);
                    Buff buff = target.BuffComponent.GetBuff((short)BuffVnums.ILLUMINATING_POWDER);
                    await target.RemoveBuffAsync(false, buff);
                    break;
                }
            case (short)SkillsVnums.CONVERT when target.BuffComponent.HasBuff((short)BuffVnums.CORRUPTION):
                {
                    int convertDamage = totalDamage / 2;
                    await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                    {
                        Damaged = target,
                        Damager = caster,
                        Damage = convertDamage,
                        CanKill = true,
                        SkillInfo = skill
                    });

                    caster.BroadcastSuPacket(target, skill, convertDamage, hitMode, true);
                    Buff buff = target.BuffComponent.GetBuff((short)BuffVnums.CORRUPTION);
                    await target.RemoveBuffAsync(false, buff);
                    break;
                }
        }
    }

    private void Skip(IBattleEntity entity, SkillInfo skillInfo)
    {
        if (!entity.IsPlayer())
        {
            return;
        }

        if (!skillInfo.Combos.Any())
        {
            return;
        }

        _skillUsageManager.ResetComboState(entity.Id);
    }

    private static bool IsReflectionNoDamage(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated) ||
        target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom);

    private static bool IsReflectionWithDamage(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.ReflectMaximumReceivedDamage);

    private static bool IsReflectionWithProbability(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.DealDamageAround, (byte)AdditionalTypes.DealDamageAround.DamageReflect);

    private async Task ProbabilityReflection(IBattleEntity caster, IBattleEntity target, DamageAlgorithmResult damageAlgorithmResult, 
        SkillInfo skill, HitInformation hitInformation, bool isCriticalBCard = false)
    {
        int randomNumber = _randomGenerator.RandomNumber();

        if (isCriticalBCard)
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.ReflectDamage,
                (byte)AdditionalTypes.ReflectDamage.ReflectDamageOnCritical, target.Level);

            if (randomNumber > firstData)
            {
                return;
            }

            int totalDamage = (int)(damageAlgorithmResult.Damages * secondData * 0.01);

            HitType hitType = damageAlgorithmResult.HitType;

            await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
            {
                Damaged = caster,
                Damager = target,
                Damage = totalDamage,
                CanKill = false,
                SkillInfo = skill
            });

            SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
            caster.BroadcastDamage(totalDamage, DmType.Reflect);
            target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);
        }
        else
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.DealDamageAround,
                (byte)AdditionalTypes.DealDamageAround.DamageReflect, target.Level);

            if (randomNumber > firstData)
            {
                return;
            }

            int totalDamage = (int)(damageAlgorithmResult.Damages * secondData * 0.01);

            HitType hitType = damageAlgorithmResult.HitType;

            await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
            {
                Damaged = caster,
                Damager = target,
                Damage = totalDamage,
                CanKill = false,
                SkillInfo = skill
            });

            caster.BroadcastDamage(totalDamage, DmType.Reflect);
            SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
            target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);
        }
    }

    private async Task ReflectDamage(IBattleEntity caster, IBattleEntity target, DamageAlgorithmResult damageAlgorithmResult, SkillInfo skill, HitInformation hitInformation,
        bool shouldDamageCaster = false)
    {
        int totalDamage = damageAlgorithmResult.Damages;
        HitType hitType = damageAlgorithmResult.HitType;

        (int firstData, int secondData) reflection =
            target.BCardComponent.GetAllBCardsInformation(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated, target.Level);
        if (reflection.firstData == 0)
        {
            reflection = target.BCardComponent.GetAllBCardsInformation(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom, target.Level);
        }

        if (shouldDamageCaster)
        {
            reflection = target.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.ReflectMaximumReceivedDamage, target.Level);
        }

        if (totalDamage > reflection.firstData)
        {
            totalDamage = reflection.firstData;
        }

        await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
        {
            Damaged = caster,
            Damager = target,
            Damage = totalDamage,
            CanKill = false,
            SkillInfo = skill
        });

        SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
        target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);

        if (!shouldDamageCaster)
        {
            if (skill.Vnum != (short)SkillsVnums.DOUBLE_RIPPER)
            {
                SuPacketHitMode reflectHitMode = skill.AoERange != 0 ? SuPacketHitMode.ReflectionAoeMiss : SuPacketHitMode.Miss;
                caster.BroadcastSuPacket(caster, skill, totalDamage, reflectHitMode, false, hitInformation.IsFirst); // Yes, it should be false for reflect.
            }

            if (hitInformation.IsFirst)
            {
                hitInformation.IsFirst = false;
            }
        }
    }

    private SuPacketHitMode GetHitMode(SkillInfo skill, HitType hitType, bool isFirst)
    {
        if (skill.TargetType == TargetType.Self && (skill.HitType == TargetHitType.EnemiesInAffectedAoE || skill.HitType == TargetHitType.AlliesInAffectedAoE))
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.MissAoe;
                case HitType.Normal:
                    return SuPacketHitMode.AttackedInAoe;
                case HitType.Critical:
                    return SuPacketHitMode.AttackedInAoeCrit;
            }
        }

        if (isFirst)
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.Miss;
                case HitType.Normal:
                    return skill.TargetType == TargetType.NonTarget ? SuPacketHitMode.AttackedInAoe : SuPacketHitMode.SuccessAttack;
                case HitType.Critical:
                    return SuPacketHitMode.CriticalAttack;
            }
        }
        else
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.MissAoe;
                case HitType.Normal:
                    return SuPacketHitMode.AttackedInAoe;
                case HitType.Critical:
                    return SuPacketHitMode.AttackedInAoeCrit;
            }
        }

        return SuPacketHitMode.SuccessAttack;
    }
}
