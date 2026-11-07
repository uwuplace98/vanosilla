using System;
using PhoenixLib.Events;
using WingsEmu.Core;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHatusHandler : IBCardEffectAsyncHandler
{
    private readonly Range<int> _blue = new()
    {
        Minimum = 18,
        Maximum = 28
    };

    private readonly IDungeonManager _dungeonManager;

    private readonly Range<int> _green = new()
    {
        Minimum = 44,
        Maximum = 54
    };

    private readonly IRandomGenerator _randomGenerator;

    private readonly Range<int> _red = new()
    {
        Minimum = 31,
        Maximum = 41
    };

    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IDamageAlgorithm _damageAlgorithm;
    private readonly IBattleEntityDumpFactory _battleEntityDumpFactory;

    public BCardHatusHandler(IRandomGenerator randomGenerator, IDungeonManager dungeonManager, IAsyncEventPipeline asyncEventPipeline, IDamageAlgorithm damageAlgorithm, IBattleEntityDumpFactory battleEntityDumpFactory)
    {
        _randomGenerator = randomGenerator;
        _dungeonManager = dungeonManager;
        _asyncEventPipeline = asyncEventPipeline;
        _damageAlgorithm = damageAlgorithm;
        _battleEntityDumpFactory = battleEntityDumpFactory;
    }

    public BCardType HandledType => BCardType.LordHatus;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skill = ctx.Skill;
        BCardDTO bCard = ctx.BCard;
        int damageDealt = ctx.DamageDealt;
        SuPacketHitMode hitMode = ctx.HitMode;
        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);


        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.LordHatus.InflictDamageAtLocation:

                int randomNumber = _randomGenerator.RandomNumber(1, 8);
                HatusState newHatusState = new()
                {
                    CastTime = TimeSpan.FromMilliseconds(2 * ctx.Skill.CastTime * 100),
                    DealtDamage = firstDataValue * 0.01
                };

                // for blue
                if ((randomNumber & 1) == 1)
                {
                    newHatusState.BlueAttack = true;
                    newHatusState.BlueX = (short)_randomGenerator.RandomNumber(_blue.Minimum, _blue.Maximum + 1);
                }

                // for red
                if ((randomNumber & 2) == 2)
                {
                    newHatusState.RedAttack = true;
                    newHatusState.RedX = (short)_randomGenerator.RandomNumber(_red.Minimum, _red.Maximum + 1);
                }

                // for green
                if ((randomNumber & 4) == 4)
                {
                    newHatusState.GreenAttack = true;
                    newHatusState.GreenX = (short)_randomGenerator.RandomNumber(_green.Minimum, _green.Maximum + 1);
                }

                _dungeonManager.AddNewHatusState(sender.MapInstance.Id, newHatusState);
                break;
            case (byte)AdditionalTypes.LordHatus.CommandSunWolf:
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }

                IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

                if (sunWolf == null)
                {
                    return;
                }
                
                IBattleEntityDump attacker = sunWolf switch
                {
                    { } mate => _battleEntityDumpFactory.Dump(mate, skill, false, sunWolf.IsSameEntity(target))
                };
                
                IBattleEntityDump defender = target switch
                {
                    IPlayerEntity character => _battleEntityDumpFactory.Dump(character, skill, true, target.IsSameEntity(target)),
                    IMonsterEntity monster => _battleEntityDumpFactory.Dump(monster, skill, true, target.IsSameEntity(target))
                };

                sunWolf.RefreshStatistics();
                DamageAlgorithmResult result = _damageAlgorithm.GenerateDamage(attacker, defender, skill, true);
                
                sunWolf.ChangePosition(new Position(target.PositionX, target.PositionY));
                sunWolf.MapInstance.Broadcast(sunWolf.GenerateTeleportPacket(target.PositionX, target.PositionY));

                _asyncEventPipeline.ProcessEventAsync(new EntityDamageEvent
                {
                     Damaged = target,
                     Damager = sunWolf,
                     Damage = result.Damages,
                     CanKill = true,
                     SkillInfo = skill
                });

                target.BroadcastDamage(result.Damages, DmType.MateDamage);
                sunWolf.BroadcastSuPacket(target, skill, result.Damages, ctx.HitMode);
                break;
            case (byte)AdditionalTypes.LordHatus.RestoreHPByDamageTaken:
                if (!sender.IsAlive())
                {
                    return;
                }

                int heal = (int)(damageDealt * (firstDataValue * 0.01));

                if (heal > secondDataValue)
                {
                    heal = secondDataValue;
                }

                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = heal
                });
                break;
        }
    }
}