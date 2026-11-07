using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Extensions;

namespace WingsEmu.Plugins.BasicImplementations.Act9.Fairies
{
    public class FairyEnchantEventHandler : IAsyncEventProcessor<FairyEnchantEvent>
    {
        private readonly WingsEmu.Game.Configurations.Act9.FairySystemConfiguration _configuration;

        public FairyEnchantEventHandler(WingsEmu.Game.Configurations.Act9.FairySystemConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task HandleAsync(FairyEnchantEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            InventoryItem item = e.Item;

            if (item?.ItemInstance?.FairyEnchantments == null ||
                item.ItemInstance.FairyEnchantments.Count == 0)
            {
                session.SendMsg("Fairy has no enchantments to change.", MsgMessageType.Middle);
                return;
            }

            var config = _configuration.Enchant;
            if (config == null)
            {
                return;
            }

            // Check gold
            if (session.PlayerEntity.Gold < config.GoldCost)
            {
                session.SendMsg("Not enough Gold.", MsgMessageType.Middle);
                return;
            }

            // Check materials
            if (config.Items != null)
            {
                foreach (var mat in config.Items)
                {
                    if (!session.PlayerEntity.HasItem(mat.Vnum, (short)mat.Amount))
                    {
                        session.SendMsg(
                            $"Not enough materials (Need {mat.Amount} of {mat.Vnum}).",
                            MsgMessageType.Middle);
                        return;
                    }
                }
            }

            // === PREVIEW ROLL (DO NOT APPLY YET) ===
            var previewEnchantments = new List<FairyEnchantmentDTO>();
            var rnd = System.Random.Shared;

            foreach (var old in item.ItemInstance.FairyEnchantments)
            {
                previewEnchantments.Add(new FairyEnchantmentDTO
                {
                    EffectId = (byte)rnd.Next(1, 10),
                    EffectValue = rnd.Next(1, 10),
                    EffectLevel = old.EffectLevel
                });
            }

            // Store preview on item
            item.ItemInstance.PendingFairyEnchantments = previewEnchantments;
            session.PlayerEntity.PendingFairyEnchantItem = item;
            // Send preview to client
            string effectsStr = string.Join(" ",
                previewEnchantments.Select(x => $"{x.EffectId}.{x.EffectValue}.{x.EffectLevel}"));

            session.SendPacket($"pa_suc 2 {previewEnchantments.Count} {effectsStr}");

            // Client will now answer with: prgb 0 44 1 0
        }
    }
}
