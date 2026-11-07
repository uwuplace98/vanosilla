using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Plugin.Fishing.Configuration;
using WingsAPI.Data.Fishing;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game._enum;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using PhoenixLib.Logging;

namespace Plugin.Fishing
{
    public interface IFishingManager
    {
        Task StartFishing(IClientSession session);
        Task ReelIn(IClientSession session);
        void StopFishing(IClientSession session);
        FishingPool GetNearbyPool(IClientSession session);
        string GenerateFishPacket(IClientSession session, FishPacketType type, int fishVnum = 0, int length = 0);
    }

    public enum FishPacketType
    {
        Login = 0,
        Fishing = 2
    }

    public class FishingManager : IFishingManager
    {
        private readonly FishingConfiguration _configuration;
        private readonly IRandomGenerator _randomGenerator;

        // OpenNOS constants  
        private const int FISHING_BUFF = 850;
        private const int BAIT_VNUM = 2614;
        private const int LINE_VNUM = 2615;
        private const int BITE_CHECK_INTERVAL_MS = 2000;
        private const int BITE_CHANCE_PERCENT = 50; // OpenNOS uses 50%
        private const int LINE_BREAK_CHANCE = 80;

        public FishingManager(FishingConfiguration configuration, IRandomGenerator randomGenerator)
        {
            _configuration = configuration;
            _randomGenerator = randomGenerator;
        }

        public FishingPool GetNearbyPool(IClientSession session)
        {
            // OpenNOS strict parity: check for exact fishing spot first
            // We use the loaded spots from configuration
            var mapId = session.CurrentMapInstance.MapId;
            var x = session.PlayerEntity.PositionX;
            var y = session.PlayerEntity.PositionY;

            // Simple distance check if strict spot not found (fallback)
            // But based on OpenNOS we should look for exact spot or radius
            // We'll stick to the "Nearby Pool" logic but using the FishingSpotDto data
            var spot = _configuration.Spots.FirstOrDefault(s => 
                s.MapId == mapId && 
                GetDistance(s.X, s.Y, x, y) <= 3);

            if (spot == null) return null;

            return new FishingPool { RegionId = spot.RegionId, X = spot.X, Y = spot.Y }; 
        }

        private FishingSpotDto GetFishingSpot(IClientSession session)
        {
            var mapId = session.CurrentMapInstance.MapId;
            var x = session.PlayerEntity.PositionX;
            var y = session.PlayerEntity.PositionY;

            return _configuration.Spots.FirstOrDefault(s => 
                s.MapId == mapId && 
                GetDistance(s.X, s.Y, x, y) <= 3);
        }

        public async Task StartFishing(IClientSession session)
        {
            var player = session.PlayerEntity;

            if (player.IsFishing)
            {
                session.SendInfoChatMessage("You are already fishing!");
                return;
            }

            var spot = GetFishingSpot(session);
            if (spot == null)
            {
                session.SendInfoChatMessage("You cannot fish here!");
                return;
            }

            if (session.PlayerEntity.CountItemWithVnum(BAIT_VNUM) < 1)
            {
                session.SendInfoChatMessage("You need bait to fish!");
                return;
            }

            if (session.PlayerEntity.CountItemWithVnum(LINE_VNUM) < 1)
            {
                session.SendInfoChatMessage("You need a fishing line!");
                return;
            }

            await session.RemoveItemFromInventory(BAIT_VNUM, 1);

            player.IsFishing = true;
            player.IsBiting = false;

            await session.PlayerEntity.AddBuffAsync(new Buff(FISHING_BUFF, player.Level));

            session.SendSuccessChatMessage("You cast your line into the water...");
            
            // OpenNOS logic: Check direction
             if (spot.Direction != session.PlayerEntity.Direction)
            {
                // In OpenNOS it forces direction. We can skip or implement if needed.
                // session.PlayerEntity.Direction = (byte)spot.Direction;
                // Broadcast Dir packet...
            }

            player.FishingCycle = Observable
                .Interval(TimeSpan.FromMilliseconds(BITE_CHECK_INTERVAL_MS))
                .Subscribe(_ => CheckForBite(session));
        }

        private void CheckForBite(IClientSession session)
        {
            var player = session.PlayerEntity;

            if (!player.IsFishing)
            {
                StopFishing(session); // Safety
                return;
            }

            if (player.IsBiting)
            {
                // Ate bait logic
                StopFishing(session);
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 0, player.Id));
                session.SendInfoChatMessage("Ugh, the fish ate the bait!");
                return;
            }

            var rnd = _randomGenerator.RandomNumber(100);
            if (rnd < BITE_CHANCE_PERCENT)
            {
                player.IsBiting = true;
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 30, player.Id, 30));
                session.SendInfoChatMessage("Something is biting! Quick, reel it in!");
            }
        }

        public async Task ReelIn(IClientSession session)
        {
            var player = session.PlayerEntity;

            if (!player.IsFishing)
            {
                 session.SendInfoChatMessage("You must be fishing to use this skill.");
                 return;
            }

            if (!player.IsBiting)
            {
                session.SendInfoChatMessage("Bad luck... Unfortunately nothing bit");
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 0, player.Id));
                // Cancel packet?
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 44, player.Id));
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(19, 0, player.Id, 2436));
                StopFishing(session);
                return;
            }

            var spot = GetFishingSpot(session);
            if (spot == null)
            {
                session.SendInfoChatMessage("You cannot fish here!");
                StopFishing(session);
                return;
            }

            // Probability Logic
            // OpenNOS: rnda = RandomDouble();
            // fishes = spot.Drops.GroupBy(Chance).OrderBy(Key)
            // prob = Key / 10
            // if rnda > prob continue
            // else pick random from group

            // Filter drops by RegionId of the spot
            var possibleDrops = _configuration.Drops
                .Where(d => d.RegionId == spot.RegionId) // Strict linking
                .ToList();

            if (!possibleDrops.Any())
            {
                // Fallback to all drops with RegionId 0? Or just fail
                possibleDrops = _configuration.Drops.Where(d => d.RegionId == 0).ToList();
            }

            FishingDropDto selectedDrop = null;
            int fishLength = 0;

            // Retry loop logic implicit in OpenNOS "goto sendFishReward" with 80% chance
            // We simulate this by attempting catch logic.
            // But strict OpenNOS: 
            // 1. Calculate drop.
            // 2. If no drop found -> 80% retry -> goto 1.
            // 3. If retry fails -> Line break.

            bool caught = false;
            int attempts = 0;
            const int MAX_ATTEMPTS = 5; // Safety cap for the "goto" loop

            while (!caught && attempts < MAX_ATTEMPTS)
            {
                attempts++;
                
                // OpenNOS: var rnda = ServerManager.RandomDouble();
                // rnda is 0.0 to 1.0
                double rnda = _randomGenerator.RandomDouble();

                var grouped = possibleDrops.GroupBy(d => d.Chance).OrderBy(g => g.Key);
                
                foreach (var group in grouped)
                {
                    // OpenNOS: double prob = (double)grouping.Key / 10;
                    // IF Chance is e.g. 5. Prob = 0.5.
                    // IF rnda (0.8) > 0.5 -> Continue
                    // IF rnda (0.3) <= 0.5 -> Catch!
                    double prob = (double)group.Key / 10.0;
                    
                    if (rnda > prob)
                    {
                        continue;
                    }

                    // Selected this group
                    var list = group.ToList();
                    selectedDrop = list[_randomGenerator.RandomNumber(list.Count)];
                    
                    // Length: Random between Min and Max+1
                    fishLength = _randomGenerator.RandomNumber(selectedDrop.MinFishLength, selectedDrop.MaxFishLength + 1);
                    caught = true;
                    break;
                }

                if (!caught)
                {
                    // 80% retry chance
                    if (_randomGenerator.RandomNumber(100) >= 80) // < 80 in OpenNOS means retry. >= 80 means fail.
                    {
                        // Failed to retry. Break loop.
                        break;
                    }
                    // Else loop continues (retry)
                }
            }

            if (!caught || selectedDrop == null)
            {
                // Line break logic
                session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 0, player.Id));
                StopFishing(session);

                // Delay 2s (handled by client visual usually, but we send packets now)
                session.SendInfoChatMessage("Oh, my fishing line broke!");
                await session.RemoveItemFromInventory(LINE_VNUM, 1);
                
                // SkillUsed packet for fail animation? 
                // OpenNOS sends SkillUsed 45
                return;
            }

            // Success
            StopFishing(session);

            // Give Item
            await session.AddItemToInventory(selectedDrop.ItemVnum, 1);

             // Log
            var log = player.FishLogs.FirstOrDefault(l => l.FishVnum == selectedDrop.ItemVnum);
            if (log == null)
            {
                log = new CharacterFishLogDto
                {
                    CharacterId = player.Id,
                    FishVnum = selectedDrop.ItemVnum,
                    CatchCount = 0,
                    MaxLength = 0
                };
                player.FishLogs.Add(log);
            }
            log.CatchCount++;
            if (fishLength > log.MaxLength) log.MaxLength = fishLength;

            // Packets
            session.SendPacket("pdti 0");
            session.SendSuccessChatMessage($"You have received this item - Fish ({fishLength}cm) x1"); 
            // Better: Get item name
            session.SendPacket($"pdti 17 {selectedDrop.ItemVnum} 1 {fishLength} -1 -1");
            session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(19, 1, player.Id, 1324));
            session.SendPacket(GenerateFishPacket(session, FishPacketType.Fishing, selectedDrop.ItemVnum, fishLength));
            
            // SP XP Logic
            // ... (Simple increment if compatible)
            session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 0, player.Id));
        }

        public void StopFishing(IClientSession session)
        {
            var player = session.PlayerEntity;
            player.FishingCycle?.Dispose();
            player.FishingCycle = null;
            player.IsFishing = false;
            player.IsBiting = false;
            player.RemoveBuff(FISHING_BUFF);
        }

        public string GenerateFishPacket(IClientSession session, FishPacketType type, int fishVnum = 0, int length = 0)
        {
            var player = session.PlayerEntity;

            switch (type)
            {
                case FishPacketType.Login:
                    var fishData = string.Join(" ", player.FishLogs.Select(log => 
                        $"{log.FishVnum}.{log.CatchCount}.{log.MaxLength}"));
                    return $"fish {(int)type} {player.FishLogs.Count} {fishData}";

                case FishPacketType.Fishing:
                    return $"fish {(int)type} {fishVnum} {length} {player.Id}";

                default:
                    return $"fish {(int)type}";
            }
        }

        private double GetDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}
