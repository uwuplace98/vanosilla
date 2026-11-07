using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act9.Fairies;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Game.Managers;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Configurations.Act9;
using WingsEmu.Game;
using WingsEmu.Packets.Enums;
using WingsEmu.DTOs.Items;
using System.Collections.Generic;
using static WingsEmu.Packets.Enums.AdditionalTypes;

namespace WingsEmu.Plugins.BasicImplementations.Act9.Fairies
{
    public class UpgradeFairiesEventHandler : IAsyncEventProcessor<UpgradeFairiesEvent>
    {
        private readonly IGameLanguageService _language;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IGameItemInstanceFactory _itemInstanceFactory;
        private readonly FairySystemConfiguration _configuration;

        public UpgradeFairiesEventHandler(
            IGameLanguageService language,
            IRandomGenerator randomGenerator,
            IGameItemInstanceFactory itemInstanceFactory,
            FairySystemConfiguration configuration)
        {
            _language = language;
            _randomGenerator = randomGenerator;
            _itemInstanceFactory = itemInstanceFactory;
            _configuration = configuration;
        }

        // Max allowed per grade
        private static readonly Dictionary<int, int> GradeLimits = new()
        {
            { 5, 2 }, // S
            { 4, 2 }, // A
            { 3, 2 }, // B
            { 1, int.MaxValue }, // C
            { 2, int.MaxValue }  // C
        };

        // Effect value ranges
        private static readonly (int min, int max)[] EffectValueRanges = new (int, int)[]
        {
            (200, 6500), (200, 6500), (1, 14), (1, 25), (1, 14),
            (6, 130), (2, 25), (1, 13), (1, 13), (1, 13),
            (1, 13), (1, 13), (1, 13), (1, 25), (1, 13),
            (1, 13), (12, 250), (1, 25), (1, 14), (1, 14), (1, 14)
        };

        public async Task HandleAsync(UpgradeFairiesEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            InventoryItem fairy = e.Fairy;

            if (fairy == null)
                return;

            // 🔒 HARD LOCK — prevents double upgrade
            lock (fairy)
            {
                int currentLevel = fairy.ItemInstance.FairyEnchantmentLevel;
                int nextLevel = currentLevel + 1;

                // Max +9
                if (nextLevel > 9)
                {
                    session.SendMsg("Fairy has reached maximum upgrade level (+9).", MsgMessageType.Middle);
                    return;
                }

                if (!_configuration.Upgrade.Levels.TryGetValue(nextLevel, out var levelConfig))
                {
                    session.SendMsg($"No configuration found for Fairy upgrade level {nextLevel}.", MsgMessageType.Middle);
                    return;
                }

                // Fixed fairy check
                if (fairy.ItemInstance.IsFixed)
                {
                    session.SendMsg("Your fairy has a fixed level.", MsgMessageType.Middle);
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }

                // Gold check
                if (session.PlayerEntity.Gold < levelConfig.GoldCost)
                {
                    session.SendMsg("Not enough Gold.", MsgMessageType.Middle);
                    return;
                }

                // Materials check
                if (levelConfig.Items != null)
                {
                    foreach (var item in levelConfig.Items)
                    {
                        if (!session.PlayerEntity.HasItem(item.Vnum, (short)item.Amount))
                        {
                            session.SendMsg($"Not enough materials (Need {item.Amount} of {item.Vnum}).", MsgMessageType.Middle);
                            return;
                        }
                    }
                }

                // Consume materials
                if (levelConfig.Items != null)
                {
                    foreach (var item in levelConfig.Items)
                    {
                        session.RemoveItemFromInventory(item.Vnum, (short)item.Amount).GetAwaiter().GetResult();
                    }
                }

                // Consume gold
                session.PlayerEntity.Gold -= levelConfig.GoldCost;
                session.RefreshGold();

                // Roll
                int roll = _randomGenerator.RandomNumber(0, 100);
                bool success = roll < levelConfig.SuccessChance;
                bool fixedLevel = !success && roll >= levelConfig.SuccessChance + levelConfig.FailChance;

                if (success)
                {
                    fairy.ItemInstance.FairyEnchantmentLevel = (byte)nextLevel;

                    fairy.ItemInstance.FairyEnchantments ??= new List<FairyEnchantmentDTO>();
                    GenerateFairyEffects(fairy.ItemInstance);

                    session.SendInventoryAddPacket(fairy);
                    session.SendMsg($"Fairy upgraded to +{nextLevel}!", MsgMessageType.Middle);
                    session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
                }
                else if (fixedLevel)
                {
                    fairy.ItemInstance.IsFixed = true;
                    session.SendMsg($"Fairy upgrade to +{nextLevel} did not change (Fixed chance).", MsgMessageType.Middle);
                }
                else
                {
                    session.SendMsg($"Fairy upgrade to +{nextLevel} failed!", MsgMessageType.Middle);
                    session.BroadcastEffectInRange(EffectType.UpgradeFail);
                }

                session.SendShopEndPacket(ShopEndType.Npc);
            }
        }

        private void GenerateFairyEffects(GameItemInstance fairy)
        {
            fairy.FairyEnchantments ??= new List<FairyEnchantmentDTO>();

            var usedEffects = fairy.FairyEnchantments.Select(f => f.EffectValue).ToHashSet();
            var gradeOptions = new List<int> { 1, 2, 3, 4, 5 };

            int grade;
            do
            {
                grade = gradeOptions[_randomGenerator.RandomNumber(0, gradeOptions.Count)];
            }
            while (fairy.FairyEnchantments.Count(f => f.EffectId == grade) >= GradeLimits[grade]);

            int effectValue;
            do
            {
                effectValue = _randomGenerator.RandomNumber(1, 22);
            }
            while (usedEffects.Contains(effectValue));

            var (min, max) = EffectValueRanges[effectValue - 1];
            int effectLevel = _randomGenerator.RandomNumber(min, max + 1);

            fairy.FairyEnchantments.Add(new FairyEnchantmentDTO
            {
                EffectId = (byte)grade,
                EffectValue = effectValue,
                EffectLevel = (byte)effectLevel
            });
        }
    }
}
