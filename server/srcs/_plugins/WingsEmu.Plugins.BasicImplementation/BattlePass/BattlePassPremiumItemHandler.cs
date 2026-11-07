using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game.Extensions;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    public class BattlePassPremiumItemHandler : IItemUsageByVnumHandler
    {
        // Battle Pass Premium activation items
        private const int BattlePassPremiumItem1 = 9283;
        private const int BattlePassPremiumItem2 = 9995;
        // Premium Battle Pass Coin - adds 50 BP points (requires premium)
        private const int PremiumBpCoin = 9284;

        public long[] Vnums => new long[] { BattlePassPremiumItem1, BattlePassPremiumItem2, PremiumBpCoin };

        public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
        {
            var itemVnum = e.Item.ItemInstance.ItemVNum;

            // Premium BP Coin - adds 50 BP points (requires premium)
            if (itemVnum == PremiumBpCoin)
            {
                if (!session.PlayerEntity.BattlePassIsPremium)
                {
                    session.SendChatMessage("You need Premium Battle Pass to use this item!", ChatMessageColorType.Red);
                    return;
                }

                // Remove the item first
                await session.RemoveItemFromInventory(itemVnum, 1);

                // Add 50 BP points
                session.PlayerEntity.BattlePassPoints += 50;

                session.SendChatMessage($"You gained 50 BP points! Total: {session.PlayerEntity.BattlePassPoints}", ChatMessageColorType.Yellow);
                Log.Info($"[BATTLEPASS] {session.PlayerEntity.Name} used Premium BP Coin (+50 BP)");
                return;
            }

            // Battle Pass Premium activation items (9283, 9995)
            if (itemVnum == BattlePassPremiumItem1 || itemVnum == BattlePassPremiumItem2)
            {
                if (session.PlayerEntity.BattlePassIsPremium)
                {
                    session.SendChatMessage("You already have Battle Pass Premium!", ChatMessageColorType.Red);
                    return;
                }

                // Activate premium
                session.PlayerEntity.BattlePassIsPremium = true;

                // Remove the item from inventory
                await session.RemoveItemFromInventory(itemVnum, 1);

                // Notify player
                session.SendChatMessage("Battle Pass Premium activated!", ChatMessageColorType.Yellow);
                session.SendMsg("Battle Pass Premium activated!", MsgMessageType.Middle);

                Log.Info($"[BATTLEPASS] {session.PlayerEntity.Name} activated Battle Pass Premium using item {itemVnum}");
            }
        }
    }
}
