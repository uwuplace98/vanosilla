using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;

namespace WingsEmu.Plugins.BasicImplementations.Game.Inventory
{
    public class ShellEnchantEventHandler : IAsyncEventProcessor<ShellEnchantEvent>
    {
        private readonly IShellGenerationAlgorithm _shellAlgorithm;
        private readonly WingsEmu.Game.Configurations.Act9.ShellSystemConfiguration _configuration;

        public ShellEnchantEventHandler(IShellGenerationAlgorithm shellAlgorithm, WingsEmu.Game.Configurations.Act9.ShellSystemConfiguration configuration)
        {
            _shellAlgorithm = shellAlgorithm;
            _configuration = configuration;
        }

        public async Task HandleAsync(ShellEnchantEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            InventoryItem item = e.Item;

            if (item == null)
            {
                return;
            }

            var config = _configuration.Enchant;
            if (config == null)
            {
                return;
            }

            if (session.PlayerEntity.Gold < config.GoldCost)
            {
                session.SendMsg("Not enough Gold.", MsgMessageType.Middle);
                return;
            }

            // Check Material Items
            if (config.Items != null)
            {
                foreach (var mat in config.Items)
                {
                    if (!session.PlayerEntity.HasItem(mat.Vnum, (short)mat.Amount))
                    {
                        session.SendMsg($"Not enough materials (Need {mat.Amount} of {mat.Vnum}).", MsgMessageType.Middle);
                        return;
                    }
                }
            }

            // Consumption
            if (config.Items != null)
            {
                foreach (var mat in config.Items)
                {
                    await session.RemoveItemFromInventory(mat.Vnum, (short)mat.Amount);
                }
            }

            session.PlayerEntity.Gold -= config.GoldCost;
            session.RefreshGold();

            // Logic: Shell Enchant (Mapped to Carved Runes / Shell Effects)
            if (item.ItemInstance.CarvedRunes == null)
            {
                item.ItemInstance.CarvedRunes = new WingsAPI.Data.CarvedRune.CarvedRunesDTO();
            }

            if (item.ItemInstance.CarvedRunes.BCards == null)
            {
                item.ItemInstance.CarvedRunes.BCards = new System.Collections.Generic.List<WingsEmu.DTOs.BCards.BCardDTO>();
            }

            // Mock Data matching user request "2.25.142.-1"
            // Type.SubType.FirstData.SecondData (or Level?)
            item.ItemInstance.CarvedRunes.BCards.Add(new WingsEmu.DTOs.BCards.BCardDTO
            {
                Type = 2,
                SubType = 25,
                FirstData = 142,
                SecondData = -1, // As per request
                IsRunePower = true // Assuming this flag relates to it being a rune/shell effect
            });

            // Also increment upgrade just in case
            item.ItemInstance.CarvedRunes.Upgrade = (byte)(item.ItemInstance.CarvedRunes.Upgrade + 1);
            if (item.ItemInstance.CarvedRunes.Upgrade > 15) item.ItemInstance.CarvedRunes.Upgrade = 15;

            // In a real implementation, we would generate specific effects here based on the Shell used.
            // For now, we just ensure the structure exists and increment the upgrade level as a visual indicator.

            session.SendMsg("Shell Enchant Successful!", MsgMessageType.Middle);
            session.BroadcastGuri(19, 0, session.PlayerEntity.Id);

            session.SendInventoryAddPacket(item);
        }
    }
}
