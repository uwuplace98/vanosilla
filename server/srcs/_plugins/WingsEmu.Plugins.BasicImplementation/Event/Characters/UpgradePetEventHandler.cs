using PhoenixLib.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TrainerSpecialist;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Event.Characters;

public class UpgradePetEventHandler : IAsyncEventProcessor<UpgradePetEvent>
{
    private readonly IGameLanguageService _gameLanguageService;
    private readonly PetMaxLevelConfiguration _configuration;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IGameLanguageService _gameLang;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public UpgradePetEventHandler(IGameLanguageService gameLanguageService, PetMaxLevelConfiguration configuration, INpcMonsterManager npcMonsterManager,
        IGameLanguageService gameLang, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguageService = gameLanguageService;
        _configuration = configuration;
        _npcMonsterManager = npcMonsterManager;
        _gameLang = gameLang;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public async Task HandleAsync(UpgradePetEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMates().FirstOrDefault(s => s.Id == e.Mate.Id);

        if (mateEntity == null)
        {
            return;
        }
        
        if (mateEntity.LastDefence.AddSeconds(4) >= DateTime.UtcNow)
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.PET_IS_FIGHTING, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.LastUpgradePet == DateTime.MinValue)
        {
            return;
        }

        session.PlayerEntity.LastUpgradePet = DateTime.MinValue;

        MaxPetLevelConfiguration infos = _configuration.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            return;
        }

        bool isRemoved = session.PlayerEntity.RemoveGold(infos.GoldRequired);

        if (!isRemoved)
        {
            return;
        }

        if (!session.PlayerEntity.HasItem(infos.ItemVnum, infos.Quantity))
        {
            return;
        }

        mateEntity.HeroLevel = 1;
        mateEntity.Stars++;
        session.SendPacket(mateEntity.GenerateIn(mateEntity.Name));
        session.PlayerEntity.MapInstance.Broadcast(mateEntity.GenerateIn(_gameLang, session.UserLanguage, _spPartnerConfiguration));
        session.SendPetInfo(mateEntity, _gameLanguageService);
        session.SendShopEndPacket(ShopEndType.SpecialistHolder);
        session.SendMsgi(MsgiMessageType.Default, Game18NConstString.StarIncreasedTo, 4, mateEntity.Stars);
        HandleQuests(session, mateEntity);
        await session.EmitEventAsync(new UpdatePetBookEvent
        {
            MateEntity = mateEntity
        });
        await session.RemoveItemFromInventory(infos.ItemVnum, infos.Quantity);
    }

    private void HandleQuests(IClientSession session, IMateEntity mateEntity)
    {
        IMonsterData originalMonster = _npcMonsterManager.GetNpc(mateEntity.MonsterVNum);

        switch (mateEntity.Stars)
        {
            case 5 when originalMonster.Stars is 1 or 2:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve75DifferentPetFrom1And2StarTo5
                });
                break;
            case 4 when originalMonster.Stars == 1:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve25PetFrom1StarTo4
                });
                break;
        }

        switch (mateEntity.Stars)
        {
            case 4 when originalMonster.Stars == 2:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve40PetFrom2StarTo4
                });
                break;
            case 6:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve1PetTo6Star
                });
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve2PetTo6Star
                });
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve3PetTo6Star
                });
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve10PetTo6Star
                });
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Evolve25PetTo6Star
                });
                switch (mateEntity.MonsterVNum)
                {
                    case 1035:
                        session.EmitEvent(new UpdateTrainerQuestEvent
                        {
                            MissionType = PetTrainerMissionType.EvolveASandDwarfTo6Stars
                        });
                        break;
                    case 478:
                        session.EmitEvent(new UpdateTrainerQuestEvent
                        {
                            MissionType = PetTrainerMissionType.EvolveAMothTo6Stars
                        });
                        break;
                    case 9:
                        session.EmitEvent(new UpdateTrainerQuestEvent
                        {
                            MissionType = PetTrainerMissionType.EvolveHappyWoolyTo6Stars
                        });
                        break;
                }

                break;
        }

        session.SendStpM();
    }
}