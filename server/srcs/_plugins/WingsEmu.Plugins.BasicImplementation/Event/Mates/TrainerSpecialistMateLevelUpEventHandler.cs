using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Game.TrainerSpecialist;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.Event.Mates
{
    public class TrainerSpecialistMateLevelUpEventHandler : IAsyncEventProcessor<TrainerSpecialistMateLevelUpEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISpPartnerConfiguration _spPartnerConfiguration;
        private readonly IBattleEntityAlgorithmService _algorithm;
        private readonly ITrainerSpecialistConfiguration _trainerSpecialistConfiguration;
        private readonly TrainerSpecialistPetSkillsLearningConfiguration _trainerSpecialistPetSkillsLearningConfiguration;
        private readonly IRandomGenerator _randomGenerator;

        public TrainerSpecialistMateLevelUpEventHandler(IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration,
            IBattleEntityAlgorithmService algorithm, ITrainerSpecialistConfiguration trainerSpecialistConfiguration,
            TrainerSpecialistPetSkillsLearningConfiguration trainerSpecialistPetSkillsLearningConfiguration, IRandomGenerator randomGenerator)
        {
            _gameLanguage = gameLanguage;
            _spPartnerConfiguration = spPartnerConfiguration;
            _algorithm = algorithm;
            _trainerSpecialistConfiguration = trainerSpecialistConfiguration;
            _trainerSpecialistPetSkillsLearningConfiguration = trainerSpecialistPetSkillsLearningConfiguration;
            _randomGenerator = randomGenerator;
        }

        public async Task HandleAsync(TrainerSpecialistMateLevelUpEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IMateEntity mateEntity = e.MateEntity;
            IMonsterEntity sparringMonster = e.SparringMonster;

            if (mateEntity == null)
            {
                return;
            }

            if (!mateEntity.IsAlive())
            {
                return;
            }

            if (sparringMonster == null)
            {
                return;
            }

            if (!sparringMonster.IsAlive())
            {
                return;
            }

            if (!sparringMonster.IsSparringMonster())
            {
                return;
            }

            HandleDefense(session, mateEntity, sparringMonster);
        }

        private void HandleDefense(IClientSession session, IMateEntity mateEntity, IMonsterEntity sparringMonster)
        {
            int experienceRequired = _algorithm.GetTrainerSpecialistExperience(mateEntity.Stars, mateEntity.HeroLevel);
            int? experienceGiven = _trainerSpecialistConfiguration.GetExperienceGivenByMonsterVnum(sparringMonster.MonsterVNum);

            if (experienceGiven.HasValue)
            {
                double firstData = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.Capture,
                    (byte)AdditionalTypes.Capture.IncreasePetsTrainingExperience, session.PlayerEntity.Level).firstData * 0.01;
                mateEntity.TrainerExperience += (int)(experienceGiven.Value * (1 + firstData));
                session.SendPetInfo(mateEntity, _gameLanguage);
            }

            if (experienceRequired > mateEntity.TrainerExperience)
            {
                return;
            }

            mateEntity.TrainerExperience = 0;
            int loyalty = mateEntity.Loyalty + 100 > 1000 ? 1000 - mateEntity.Loyalty : 100;
            mateEntity.Loyalty += (short)loyalty;
            mateEntity.Experience = 0;
            mateEntity.HeroLevel++;
            mateEntity.RefreshMaxHpMp(_algorithm);
            session.RefreshParty(_spPartnerConfiguration);
            mateEntity.Hp = mateEntity.MaxHp;
            mateEntity.Mp = mateEntity.MaxMp;
            session.SendPetInfo(mateEntity, _gameLanguage);
            session.BroadcastEffectInRange(EffectType.NormalLevelUp);
            session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
            HandleQuests(session, mateEntity);

            session.EmitEvent(new UpdatePetBookEvent
            {
                MateEntity = mateEntity
            });
            session.EmitEvent(new HeroicLevelUpMateEvent
            {
                HeroLevel = mateEntity.HeroLevel,
                LevelUpType = MateLevelUpType.Normal,
                NosMateMonsterVnum = mateEntity.NpcMonsterVNum
            });
            
            int mateHeroLevel = mateEntity.HeroLevel;
            HeroicLevelConfiguration validHeroicLevel = _trainerSpecialistPetSkillsLearningConfiguration.Skills.FirstOrDefault(skillConfig => skillConfig.HeroicLevel == mateHeroLevel);
            bool isValidHeroicUpgradeSkillsLevel = _trainerSpecialistPetSkillsLearningConfiguration.UpgradeSkillsHeroLevel.Contains(mateHeroLevel);
            int? firstHeroicLevel = _trainerSpecialistPetSkillsLearningConfiguration.GetFirstHeroicLevel();

            if (validHeroicLevel != null)
            {
                int currentSkillsCount = mateEntity.TrainerSkills.Count;
                int maxSkillsLimit = 2;
                int skillsToAdd = maxSkillsLimit - currentSkillsCount;

                if (firstHeroicLevel.HasValue)
                {
                    if (validHeroicLevel.HeroicLevel == firstHeroicLevel.Value && mateEntity.TrainerSkills.Any())
                    {
                        return;
                    }
                }

                if (skillsToAdd > 0)
                {
                    var existingTrainerSkillVnums = new HashSet<int>(mateEntity.TrainerSkills);
                    var possibleSkills = validHeroicLevel.PossibleSkills
                        .Where(skill => !existingTrainerSkillVnums.Contains(skill))
                        .ToList();

                    if (possibleSkills.Count > 0)
                    {
                        int randomIndex = _randomGenerator.RandomNumber(possibleSkills.Count);
                        int selectedSkill = possibleSkills[randomIndex];
                        mateEntity.TrainerSkills.Add(selectedSkill);
                        var monsterSkill = new NpcMonsterSkill(StaticSkillsManager.Instance.GetSkill(selectedSkill), 100, false, false);
                        mateEntity.Skills.Add(monsterSkill);
                        session.SendPetInfo(mateEntity, _gameLanguage);

                        if (mateEntity.IsTeamMember)
                        {
                            session.SendMateSkillPacket(mateEntity);
                        }
                    }
                }
            }

            if (isValidHeroicUpgradeSkillsLevel)
            {
                int randomIndex = _randomGenerator.RandomNumber(0, mateEntity.TrainerSkills.Count);
                IBattleEntitySkill oldSkill = mateEntity.Skills.FirstOrDefault(skill => skill.Skill.Id == mateEntity.TrainerSkills[randomIndex]);
                mateEntity.TrainerSkills[randomIndex]++;

                if (oldSkill != null)
                {
                    oldSkill.Skill.Id = mateEntity.TrainerSkills[randomIndex];
                }

                session.SendPetInfo(mateEntity, _gameLanguage);

                if (mateEntity.IsTeamMember)
                {
                    session.SendMateSkillPacket(mateEntity);
                }
            }
        }

        private void HandleQuests(IClientSession session, IMateEntity mateEntity)
        {
            if (mateEntity.Level == 60 && mateEntity.Stars == 6)
            {
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.ReachTrainingLevel60With200Pets
                });
            }
            session.SendStpM();
        }
    }
}