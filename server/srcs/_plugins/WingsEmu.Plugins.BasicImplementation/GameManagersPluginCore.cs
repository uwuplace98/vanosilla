using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WingsAPI.Packets.Handling;
using PhoenixLib.Caching;
using PhoenixLib.Configuration;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.DAL.Redis.Locks;
using WingsAPI.Plugins;
using WingsEmu.Game;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Act7.CarvedRunes;
using WingsEmu.Game.Configurations.Act7.Tattoos;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Features;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Relations;
using WingsEmu.Game.Skills;
using WingsEmu.Game.SnackFood;
using WingsEmu.Plugins.BasicImplementations.Algorithms;
using WingsEmu.Plugins.BasicImplementations.Arena;
using WingsEmu.Plugins.BasicImplementations.Bazaar;
using WingsEmu.Plugins.BasicImplementations.Compliments;
using WingsEmu.Plugins.BasicImplementations.DbServer;
using WingsEmu.Plugins.BasicImplementations.Entities;
using WingsEmu.Plugins.BasicImplementations.Event.Items;
using WingsEmu.Plugins.BasicImplementations.Factories;
using WingsEmu.Plugins.BasicImplementations.ForbiddenNames;
using WingsEmu.Plugins.BasicImplementations.InterChannel;
using WingsEmu.Plugins.BasicImplementations.Inventory;
using WingsEmu.Plugins.BasicImplementations.ItemUsage;
using WingsEmu.Plugins.BasicImplementations.Mail;
using WingsEmu.Plugins.BasicImplementations.Managers;
using WingsEmu.Plugins.BasicImplementations.Managers.StaticData;
using WingsEmu.Plugins.BasicImplementations.Miniland;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Maps;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Monsters;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Portals;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;
using WingsEmu.Plugins.BasicImplementations.Ship;
using WingsEmu.Plugins.GameEvents;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Act9.Fairies;
using WingsEmu.Plugins.BasicImplementations.Act9.Fairies;
using WingsEmu.Plugins.BasicImplementations.Game.Inventory;
using PhoenixLib.Events;
using WingsEmu.Plugins.BasicImplementation.BattlePass;
using WingsEmu.Game._packetHandling;
using WingsEmu.Plugins.BasicImplementation.PacketHandling;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Plugins.BasicImplementations.Managers.StaticData;
using WingsEmu.Plugins.BasicImplementation.NosMall;

namespace WingsEmu.Plugins.BasicImplementations;

public class GameManagersPluginCore : IGameServerPlugin
{
    public string Name => nameof(GameManagersPluginCore);


    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        // server configs
        services.AddConfigurationsFromDirectory<TeleporterImportFile>("map_teleporters");
        services.TryAddSingleton<ITeleporterManager, TeleporterManager>();

        services.AddConfigurationsFromDirectory<RandomBoxImportFile>("random_boxes");
        services.AddConfigurationsFromDirectory<ItemBoxImportFile>("item_boxes");
        services.TryAddSingleton<IItemBoxManager, ItemBoxManager>();

        services.AddConfigurationsFromDirectory<RecipeImportFile>("recipes");
        services.TryAddSingleton<IRecipeManager, RecipeManager>();
        services.TryAddSingleton<IRecipeFactory, RecipeFactory>();

        services.AddConfigurationsFromDirectory<DropImportFile>("global_drops");
        services.TryAddSingleton<IDropManager, DropManager>();

        services.AddConfigurationsFromDirectory<MapNpcImportFile>("map_npc_placement");
        services.TryAddSingleton<IMapNpcManager, MapNpcManager>();
        services.TryAddSingleton<IShopManager, ShopManager>();

        services.AddConfigurationsFromDirectory<MapMonsterImportFile>("map_monster_placement");
        services.TryAddSingleton<IMapMonsterManager, MapMonsterManager>();

        services.AddConfigurationsFromDirectory<PortalImportFile>("map_portals");
        services.AddConfigurationsFromDirectory<ConfiguredMapImportFile>("maps");
        services.TryAddSingleton<IMapManager, MapManager>();

        // core client data
        services.TryAddSingleton<ICardsManager, CardsManager>();
        services.TryAddSingleton<INpcMonsterManager, NpcMonsterManager>();
        services.TryAddSingleton<ISkillsManager, SkillsManager>();
        services.TryAddSingleton<ISkillsManager, SkillsManager>();
        services.TryAddSingleton<IItemsManager, ItemsManager>();
        services.TryAddSingleton<INosMallManager, NosMallManager>();
        services.TryAddSingleton<INosMallWishlistManager, NosMallWishlistManager>();

        // mails 
        services.TryAddSingleton<MailCreationManager>();
        services.AddHostedService(s => s.GetRequiredService<MailCreationManager>());

        // other managers
        services.TryAddSingleton<IArenaManager, ArenaManager>();
        services.TryAddSingleton<ITeleportManager, TeleportManager>();
        services.AddBazaarModule();
        services.AddInterChannelModule();
        services.AddShipModule(gameServer);
        services.AddDbServerModule();
        services.TryAddSingleton<IRankingManager, RankingManager>();
        services.TryAddSingleton<IExpirableLockService, RedisCheckableLock>();
        services.TryAddSingleton<IServerManager, ServerManager>();
        services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));
        services.TryAddSingleton(typeof(IUuidKeyCachedRepository<>), typeof(InMemoryUuidCacheRepository<>));
        services.TryAddSingleton(typeof(IKeyValueCache<>), typeof(InMemoryKeyValueCache<>));
        services.TryAddSingleton<IRandomGenerator, RandomGenerator>();
        services.TryAddSingleton<ISessionManager, SessionManager>();
        services.TryAddSingleton<IShopFactory, ShopFactory>();
        services.TryAddSingleton<IGroupManager, GroupManager>();
        services.TryAddSingleton<IScriptedInstanceManager, ScriptedInstanceManager>();
        services.TryAddSingleton<IMinilandManager, MinilandManager>();
        services.TryAddSingleton<IMinigameManager, MinigameManager>();
        services.TryAddSingleton<IDelayConfiguration, DelayConfiguration>();
        services.TryAddSingleton<IDelayManager, DelayManager>();
        services.TryAddSingleton<IMateTransportFactory, MateTransportFactory>();
        services.TryAddSingleton<IGameEventRegistrationManager, GameEventRegistrationManager>();
        services.TryAddSingleton<IRevivalManager, RevivalManager>();
        services.TryAddSingleton<ISacrificeManager, SacrificeManager>();
        services.TryAddSingleton<IMeditationManager, MeditationManager>();
        services.TryAddSingleton<IPhantomPositionManager, PhantomPositionManager>();
        services.TryAddSingleton<IInvitationManager, InvitationManager>();
        services.TryAddSingleton<IGroupFactory, GroupFactory>();
        services.TryAddSingleton<IGameItemInstanceFactory, GameItemInstanceFactory>();
        services.TryAddSingleton<ICellonGenerationAlgorithm, CellonGenerationAlgorithm>();
        services.TryAddSingleton<IShellGenerationAlgorithm, ShellGenerationAlgorithm>();
        services.TryAddSingleton<IDamageAlgorithm, DamageAlgorithm>();
        services.TryAddSingleton<ISpyOutManager, SpyOutManager>();
        services.TryAddSingleton<IComplimentsManager, ComplimentsManager>();
        services.AddTransient<IBuffFactory, BuffFactory>();
        services.AddTransient<IFoodSnackComponentFactory, FoodSnackComponentFactory>();
        services.AddTransient<IMapDesignObjectFactory, MapDesignObjectFactory>();
        services.AddTransient<IBattleEntityDumpFactory, BattleEntityDumpFactory>();
        services.TryAddSingleton<IPlayerEntityFactory, PlayerEntityFactory>();
        services.TryAddSingleton<IMateEntityFactory, MateEntityFactory>();
        services.TryAddSingleton<IItemUsageToggleManager, RedisItemUsageToggleManager>();
        services.TryAddSingleton<IGameFeatureToggleManager, RedisGameFeatureToggleManager>();

        services.TryAddSingleton<IForbiddenNamesManager, ReloadableForbiddenNamesManager>();

        services.AddFileConfiguration<Act4Configuration>();
        services.AddFileConfiguration<SnackFoodConfiguration>("snack_food_configuration");

        services.AddFileConfiguration<RelictConfiguration>("relict_configuration");
        services.AddFileConfiguration<ItemSumConfiguration>("item_sum_configuration");
        services.AddFileConfiguration<UpgradeNormalItemConfiguration>("upgrade_normal_item_configuration");
        services.AddFileConfiguration<UpgradePhenomenalItemConfiguration>("upgrade_phenomenal_item_configuration");

        services.AddFileConfiguration<GamblingRarityInfo>("gambling_configuration");
        services.TryAddSingleton<IGamblingRarityConfiguration, GamblingRarityConfiguration>();

        services.AddFileConfiguration<DropRarityConfiguration>("drop_rarity_configuration");
        services.TryAddSingleton<IDropRarityConfigurationProvider, DropRarityConfigurationProvider>();

        services.AddFileConfiguration<PartnerSpecialistSkillRollConfiguration>();
        services.TryAddSingleton<IPartnerSpecialistSkillRoll, PartnerSpecialistSkillRoll>();

        services.AddMultipleConfigurationOneFile<TimeSpaceFileConfiguration>("time_space_configuration");
        services.TryAddSingleton<ITimeSpaceConfiguration, TimeSpaceConfiguration>();

        services.AddMultipleConfigurationOneFile<TimeSpaceNpcRunConfiguration>("time_space_npc_run_configuration");
        services.TryAddSingleton<ITimeSpaceNpcRunConfig, TimeSpaceNpcRunConfig>();

        services.AddMultipleConfigurationOneFile<ChestDropItemConfiguration>("chest_drop_item_configuration");
        services.TryAddSingleton<IChestDropItemConfig, ChestDropItemConfig>();

        services.AddMultipleConfigurationOneFile<SubActsConfiguration>("subacts_configuration");
        services.TryAddSingleton<ISubActConfiguration, SubActConfiguration>();

        services.AddFileConfiguration<BuffsToRemoveConfiguration>("buffs_to_remove_configuration");
        services.TryAddSingleton<IBuffsToRemoveConfig, BuffsToRemoveConfig>();

        services.AddMultipleConfigurationOneFile<GibberishConfiguration>("gibberish_configuration");
        services.TryAddSingleton<IGibberishConfig, GibberishConfig>();

        services.AddMultipleConfigurationOneFile<Act5NpcRunCraftItemConfig>("act5_npc_run_item_configuration");
        services.TryAddSingleton<IAct5NpcRunCraftItemConfiguration, Act5NpcRunCraftItemConfiguration>();

        services.AddMultipleConfigurationOneFile<PartnerSpecialistBasicConfiguration>("partner_specialist_basic_configuration");
        services.TryAddSingleton<IPartnerSpecialistBasicConfig, PartnerSpecialistBasicConfig>();

        services.AddMultipleConfigurationOneFile<MonsterTalkingConfiguration>("monster_talking_configuration");
        services.TryAddSingleton<IMonsterTalkingConfig, MonsterTalkingConfig>();

        services.AddFileConfiguration<InitialQuestConfiguration>("initial_quest_configuration");
        services.TryAddSingleton<IInitialQuestConfig, InitialQuestConfig>();

        services.AddFileConfiguration<RainbowBattleConfiguration>("rainbow_configuration");

        services.AddFileConfiguration<CreateFairyAct6Configuration>("act6_crafting_fairy_configuration");

        services.AddFileConfiguration<PetMaxLevelConfiguration>("trainer_specialist/monster_max_level");

        services.AddFileConfiguration<TrainerConfig>("trainer_config");
        services.TryAddSingleton<ITrainerConfiguration, TrainerConfiguration>();

        services.AddFileConfiguration<TrainerSpecialistFileConfiguration>("trainer_specialist/monster_spawn");
        services.TryAddSingleton<ITrainerSpecialistConfiguration, TrainerSpecialistConfiguration>();

        services.AddFileConfiguration<TrainerQuestConfiguration>("trainer_specialist/trainer_quest");

        services.AddFileConfiguration<TrainerSpecialistPetBookConfiguration>("trainer_specialist/pet_book");
        services.AddFileConfiguration<TrainerSpecialistPetSkillsLearningConfiguration>("trainer_specialist/pet_skills_learning");

        services.AddFileConfiguration<ItemBuyDailyLimitConfiguration>("item_buy_daily_limit");

        services.AddFileConfiguration<TattooOptionsConfiguration>("act7/tattoo/tattoo_options");
        services.AddFileConfiguration<CraftTattooItemsConfiguration>("act7/tattoo/craft_tattoo");
        services.AddFileConfiguration<TattooUpgradeConfiguration>("act7/tattoo/tattoo_upgrade");
        services.AddFileConfiguration<CarvedRuneUpgradeConfiguration>("act7/carved_runes/rune_upgrade");
        services.AddFileConfiguration<WeaponRuneCardConfiguration>("act7/carved_runes/weapon_rune_card");
        services.AddFileConfiguration<ArmorRuneCardConfiguration>("act7/carved_runes/armor_rune_card");
        services.AddFileConfiguration<LandOfDeathConfiguration>("lod_configuration");

        services.AddFileConfiguration<WingsEmu.Game.Configurations.Act9.FairySystemConfiguration>("act9_fairy_system");
        services.AddFileConfiguration<WingsEmu.Game.Configurations.Act9.ShellSystemConfiguration>("act9_shell_system");



        services.TryAddSingleton<IBattlePassManager, BattlePassManager>();

        services.AddSingleton(new RegisteredPacketHandler { HandlerType = typeof(BpOpenPacketHandler) });
        services.AddTransient<BpOpenPacketHandler>();

        services.AddSingleton(new RegisteredPacketHandler { HandlerType = typeof(BpMSelPacketHandler) });
        services.AddTransient<BpMSelPacketHandler>();

        services.AddSingleton(new RegisteredPacketHandler { HandlerType = typeof(BpPSelPacketHandler) });
        services.AddTransient<BpPSelPacketHandler>();

        services.AddSingleton(new RegisteredPacketHandler { HandlerType = typeof(BpClosePacketHandler) });
        services.AddTransient<BpClosePacketHandler>();

        services.AddSingleton(new RegisteredPacketHandler { HandlerType = typeof(MallPacketHandler) });
        services.AddTransient<MallPacketHandler>();


    }
}