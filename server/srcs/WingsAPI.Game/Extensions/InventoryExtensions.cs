using System.Threading.Tasks;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Extensions
{
    public static class InventoryExtensions
    {
        public static async Task AddItemToInventory(this IClientSession session, int vnum, int amount = 1)
        {
            var instance = new GameItemInstance(vnum, amount, 0, 0, 0);
            var inventoryItem = new InventoryItem { ItemInstance = instance };
            await session.EmitEventAsync(new InventoryAddItemEvent(inventoryItem, true));
        }
    }
}
