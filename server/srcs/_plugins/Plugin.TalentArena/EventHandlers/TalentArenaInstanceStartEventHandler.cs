using PhoenixLib.Events;
using Plugin.TalentArena.Managers;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Plugins.BasicImplementations.Factories;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaInstanceStartEventHandler : IAsyncEventProcessor<TalentArenaInstanceStartEvent>
{
    private readonly ITalentArenaManager _talentArenaManager;
    private readonly ITalentArenaFactory _talentArenaFactory;
    private readonly IMapDesignObjectFactory _mapDesignObjectFactory;

    public TalentArenaInstanceStartEventHandler(ITalentArenaManager talentArenaManager, ITalentArenaFactory talentArenaFactory, IMapDesignObjectFactory mapDesignObjectFactory)
    {
        _talentArenaManager = talentArenaManager;
        _talentArenaFactory = talentArenaFactory;
        _mapDesignObjectFactory = mapDesignObjectFactory;
    }

    public async Task HandleAsync(TalentArenaInstanceStartEvent e, CancellationToken cancellation)
    {
        TalentArenaInstance talentArenaInstance = await _talentArenaFactory.CreateTalentArenaInstance(e.ZenasTeam, e.EreniaTeam);

        if (talentArenaInstance == null)
        {
            return;
        }

        _talentArenaManager.AddArenaInstance(talentArenaInstance);

        for (int i = 0; i < 6; i++)
        {
            MapDesignObject mapObject = _mapDesignObjectFactory.CreateTalentArenaGameObject(talentArenaInstance.ZenasTeam.Members.FirstOrDefault()!.PlayerEntity.Id, (short)(4433 + (i > 2 ? 5 - i : i)), (byte)i, (short)(i > 2 ? 120 : 19), (short)(i > 2 ? 35 + i % 3 * 4 : 36 + i % 3 * 4));

            talentArenaInstance.MapInstance.MapDesignObjects.Add(mapObject);
        }

        talentArenaInstance.SecondsAmount = TimeSpan.FromSeconds(60);

        await HandleStart(talentArenaInstance.ZenasTeam, talentArenaInstance);
        await HandleStart(talentArenaInstance.EreniaTeam, talentArenaInstance);
    }

    public async Task HandleStart(TalentArenaTeam talentArenaTeam, TalentArenaInstance instance)
    {
        IMapInstance mapInstance = instance.MapInstance;

        foreach (IClientSession member in talentArenaTeam.Members)
        {
            ITalentArenaComponent teamMember = member.PlayerEntity.TalentArenaComponent;
            member.PlayerEntity.TalentArenaComponent.SetTalentArena(instance, teamMember.TeamType);

            member.PlayerEntity.Hp = member.PlayerEntity.MaxHp;
            member.PlayerEntity.Mp = member.PlayerEntity.MaxMp;

            await member.PlayerEntity.RemovePositiveBuffs(100);
            await member.PlayerEntity.RemoveNegativeBuffs(100);
            await member.EmitEventAsync(new RemoveVehicleEvent());

            member.PlayerEntity.ClearSkillCooldowns();
            foreach (IBattleEntitySkill skill in member.PlayerEntity.Skills)
            {
                skill.LastUse = DateTime.MinValue;
                member.SendSkillCooldownReset(skill.Skill.CastId);
            }

            foreach (IMateEntity mate in member.PlayerEntity.MateComponent.TeamMembers())
            {
                await member.EmitEventAsync(new MateLeaveTeamEvent
                {
                    MateEntity = mate
                });

                if (mate.IsAlive())
                {
                    continue;
                }

                mate.Hp = 1;
                mate.Mp = 1;
            }

            short randomX;
            short randomY;

            switch (teamMember.TeamType)
            {
                case ArenaTeamType.ZENAS:
                    randomX = 125;
                    randomY = 38;

                    break;

                case ArenaTeamType.ERENIA:

                    randomX = 14;
                    randomY = 38;

                    break;

                default:
                    member.PlayerEntity.TalentArenaComponent.RemoveTalentArena();
                    continue;
            }

            member.ChangeMap(mapInstance, randomX, randomY);

            member.SendPacket(member.GenerateTaM(0));
            member.SendPacket(TalentArenaExtensions.GenerateTaSv(0));
            member.SendPacket(TalentArenaExtensions.GenerateTaSt(TalentArenaOptionType.Watch));

            member.SendPacket(member.GenerateTaM(3, (short?)instance.SecondsAmount.TotalSeconds));
            member.SendPacket(member.GenerateTaP(0, false));
        }
    }
}