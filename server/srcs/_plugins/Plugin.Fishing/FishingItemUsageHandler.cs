using System.Threading.Tasks;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game._ItemUsage;

namespace Plugin.Fishing
{
    public class FishingItemUsageHandler : IItemUsageByVnumHandler
    {
        private readonly IFishingManager _fishingManager;

        public FishingItemUsageHandler(IFishingManager fishingManager)
        {
            _fishingManager = fishingManager;
        }

        // Fishing Rod VNums:
        // 4958: Basic Fishing Rod
        // 4959: Pro Fishing Rod
        // 4960: Expert Fishing Rod
        public long[] Vnums => new long[] { 4958, 4959, 4960 };

        public async Task HandleAsync(IClientSession session, InventoryItem inventoryItem)
        {
            // Check if already fishing - if so, reel in
            if (session.PlayerEntity.IsFishing)
            {
                await _fishingManager.ReelIn(session);
            }
            else
            {
                await _fishingManager.StartFishing(session);
            }
        }
    }
}
