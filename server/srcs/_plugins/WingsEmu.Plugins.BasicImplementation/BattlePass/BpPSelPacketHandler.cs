using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.BasicImplementation.Extensions;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    public class BpPSelPacketHandler : GenericGamePacketHandlerBase<BpPSelPacket>
    {
        private readonly IBattlePassManager _manager;
        private readonly IGameItemInstanceFactory _itemFactory;

        public BpPSelPacketHandler(IBattlePassManager manager, IGameItemInstanceFactory itemFactory)
        {
            _manager = manager;
            _itemFactory = itemFactory;
        }

        protected override async Task HandlePacketAsync(IClientSession session, BpPSelPacket packet)
        {
            Log.Debug($"[BATTLEPASS_REWARD] Received Request: BearingId={packet.BearingId}, Type={packet.Type}");

            var character = session.PlayerEntity;
            var bearingConf = _manager.BearingConfig;

            if (bearingConf?.Bearings == null || bearingConf.Bearings.Count == 0) return;

            // Type 2 = Get all available
            if (packet.Type == WingsAPI.Packets.Enums.BattlePass.BattlePassItemType.GetAll)
            {
                await ClaimAllAvailable(session);
                return;
            }

            // Find the bearing
            var bearing = bearingConf.Bearings.Find(b => b.Id == packet.BearingId);
            if (bearing == null)
            {
                Log.Warn($"[BATTLEPASS_REWARD] Bearing {packet.BearingId} not found");
                return;
            }

            // Check points
            if (character.BattlePassPoints < bearing.MinimumBattlepassPoint)
            {
                Log.Debug($"[BATTLEPASS_REWARD] Not enough points: {character.BattlePassPoints} < {bearing.MinimumBattlepassPoint}");
                return;
            }

            bool isPremiumReward = packet.Type == WingsAPI.Packets.Enums.BattlePass.BattlePassItemType.Premium;
            int claimId = isPremiumReward ? bearing.Id + 10000 : bearing.Id; // Use offset for premium tracking

            // Check if already claimed
            if (character.BattlePassClaimedRewards != null && character.BattlePassClaimedRewards.Contains(claimId))
            {
                Log.Debug($"[BATTLEPASS_REWARD] Bearing {claimId} already claimed");
                return;
            }

            // Check Premium for premium rewards
            if (isPremiumReward && !character.BattlePassIsPremium)
            {
                Log.Debug($"[BATTLEPASS_REWARD] Premium required for premium reward");
                return;
            }

            // Get reward VNum and amount
            int rewardVnum = isPremiumReward ? bearing.PremiumRewardVnum : bearing.RewardVnum;
            int rewardAmount = isPremiumReward ? bearing.PremiumRewardAmount : bearing.RewardAmount;

            if (rewardVnum <= 0)
            {
                Log.Warn($"[BATTLEPASS_REWARD] No reward configured for bearing {bearing.Id} (premium={isPremiumReward})");
                return;
            }

            // Check inventory space
            if (!character.HasSpaceFor(rewardVnum, (short)rewardAmount))
            {
                session.SendChatMessage("Not enough inventory space!", ChatMessageColorType.Red);
                return;
            }

            // Mark as claimed
            if (character.BattlePassClaimedRewards == null) character.BattlePassClaimedRewards = new List<int>();
            character.BattlePassClaimedRewards.Add(claimId);

            // Give item
            var newItem = _itemFactory.CreateItem(rewardVnum, rewardAmount);
            await session.AddNewItemToInventory(newItem);

            Log.Info($"[BATTLEPASS_REWARD] {character.Name} claimed bearing {bearing.Id} (premium={isPremiumReward}): {rewardVnum} x{rewardAmount}");

            // Refresh Battle Pass UI
            session.SendPacket(_manager.GenerateBpp(character));
        }

        private async Task ClaimAllAvailable(IClientSession session)
        {
            var character = session.PlayerEntity;
            var bearingConf = _manager.BearingConfig;

            if (character.BattlePassClaimedRewards == null)
                character.BattlePassClaimedRewards = new List<int>();

            int claimedCount = 0;

            foreach (var bearing in bearingConf.Bearings)
            {
                // Skip if not enough points
                if (character.BattlePassPoints < bearing.MinimumBattlepassPoint)
                    continue;

                // Claim free reward if not yet claimed
                if (!character.BattlePassClaimedRewards.Contains(bearing.Id) && bearing.RewardVnum > 0)
                {
                    if (character.HasSpaceFor(bearing.RewardVnum, (short)bearing.RewardAmount))
                    {
                        character.BattlePassClaimedRewards.Add(bearing.Id);
                        var newItem = _itemFactory.CreateItem(bearing.RewardVnum, bearing.RewardAmount);
                        await session.AddNewItemToInventory(newItem);
                        claimedCount++;
                    }
                }

                // Claim premium reward if premium and not yet claimed
                int premiumClaimId = bearing.Id + 10000;
                if (character.BattlePassIsPremium && !character.BattlePassClaimedRewards.Contains(premiumClaimId) && bearing.PremiumRewardVnum > 0)
                {
                    if (character.HasSpaceFor(bearing.PremiumRewardVnum, (short)bearing.PremiumRewardAmount))
                    {
                        character.BattlePassClaimedRewards.Add(premiumClaimId);
                        var newItem = _itemFactory.CreateItem(bearing.PremiumRewardVnum, bearing.PremiumRewardAmount);
                        await session.AddNewItemToInventory(newItem);
                        claimedCount++;
                    }
                }
            }

            if (claimedCount > 0)
            {
                Log.Info($"[BATTLEPASS_REWARD] {character.Name} claimed all: {claimedCount} rewards");
                session.SendChatMessage($"Claimed {claimedCount} rewards!", ChatMessageColorType.Yellow);
            }

            // Refresh Battle Pass UI
            session.SendPacket(_manager.GenerateBpp(character));
        }
    }
}

