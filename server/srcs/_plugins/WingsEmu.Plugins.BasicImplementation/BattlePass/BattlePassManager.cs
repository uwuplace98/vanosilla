using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Configuration;
using PhoenixLib.Logging;
using WingsAPI.Data.Character;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    public interface IBattlePassManager
    {
        BattlePassMainConfiguration MainConfig { get; }
        BattlePassBearingConfiguration BearingConfig { get; }
        BattlePassQuestConfiguration QuestConfig { get; }
        void LoadConfiguration();
        Task RerollQuest(IClientSession session, long oldQuestId);
    }

    public class BattlePassManager : IBattlePassManager
    {
        private readonly IGameLanguageService _languageService;
        private readonly IConfigurationPathProvider _pathProvider;

        private BattlePassMainConfiguration _mainConfig;
        private BattlePassBearingConfiguration _bearingConfig;
        private BattlePassQuestConfiguration _questConfig;

        public BattlePassMainConfiguration MainConfig => _mainConfig;
        public BattlePassBearingConfiguration BearingConfig => _bearingConfig;
        public BattlePassQuestConfiguration QuestConfig => _questConfig;

        public BattlePassManager(IGameLanguageService languageService, IConfigurationPathProvider configurationPathProvider)
        {
            _languageService = languageService;
            _pathProvider = configurationPathProvider;
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            // Load main configuration
            string mainPath = _pathProvider.GetConfigurationPath("battlepass_configuration.yaml");
            _mainConfig = LoadFile<BattlePassMainConfiguration>(deserializer, mainPath) ?? new BattlePassMainConfiguration();

            // Load bearings (tiers)
            string bearingPath = _pathProvider.GetConfigurationPath("battlepass_bearing.yaml");
            _bearingConfig = LoadFile<BattlePassBearingConfiguration>(deserializer, bearingPath) ?? new BattlePassBearingConfiguration();

            // Load quests
            string questPath = _pathProvider.GetConfigurationPath("battlepass_quest.yaml");
            _questConfig = LoadFile<BattlePassQuestConfiguration>(deserializer, questPath) ?? new BattlePassQuestConfiguration();

            Log.Info($"[BattlePassManager] Configuration loaded. Bearings: {_bearingConfig.Bearings?.Count ?? 0}, Quests: {_questConfig.Quests?.Count ?? 0}");
        }

        private T LoadFile<T>(IDeserializer deserializer, string path) where T : class
        {
            try
            {
                if (!File.Exists(path))
                {
                    Log.Warn($"[BattlePassManager] File not found: {path}");
                    return null;
                }

                string yaml = File.ReadAllText(path);
                return deserializer.Deserialize<T>(yaml);
            }
            catch (Exception ex)
            {
                Log.Error($"[BattlePassManager] Failed to load {path}: {ex.Message}", ex);
                return null;
            }
        }

        public async Task RerollQuest(IClientSession session, long oldQuestId)
        {
            if (_questConfig?.Quests == null || !_questConfig.Quests.Any())
            {
                session.SendChatMessage("No quests configured.", ChatMessageColorType.Red);
                return;
            }

            var character = session.PlayerEntity;
            var currentQuest = character.BattlePassQuests?.FirstOrDefault(x => x.QuestId == oldQuestId);
            if (currentQuest == null)
            {
                Log.Warn($"[BattlePass] Reroll failed. Quest {oldQuestId} not found for user.");
                return;
            }

            // Find quest definition
            var questDef = _questConfig.Quests.FirstOrDefault(q => q.Id == currentQuest.QuestId);
            if (questDef == null)
            {
                Log.Warn($"[BattlePass] Quest definition {oldQuestId} not found in config.");
                return;
            }

            // Find replacement - filter by same frequency type (Daily/Weekly)
            var candidates = _questConfig.Quests
                .Where(q => q.FrequencyType == questDef.FrequencyType && q.Id != questDef.Id)
                .ToList();

            // Exclude currently active quests
            var activeIds = character.BattlePassQuests.Select(x => x.QuestId).ToHashSet();
            candidates = candidates.Where(q => !activeIds.Contains(q.Id)).ToList();

            if (!candidates.Any())
            {
                session.SendChatMessage("No other quests available to reroll.", ChatMessageColorType.Yellow);
                return;
            }

            // Pick random
            var newQuestDef = candidates[new Random().Next(candidates.Count)];

            character.BattlePassQuests.Remove(currentQuest);
            character.BattlePassQuests.Add(new CharacterBattlePassQuestDto
            {
                QuestId = newQuestDef.Id,
                Progress = 0,
                IsFinished = false
            });

            session.SendChatMessage($"Quest rerolled! New quest: {newQuestDef.Id}", ChatMessageColorType.Green);
        }
    }
}
