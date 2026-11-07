using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.Fishing;
using WingsAPI.Data.GameData;

namespace Plugin.ResourceLoader.Loaders
{
    /// <summary>
    /// Loads fishing spot and drop data from fish.dat file.
    /// File format:
    /// MAP {MapVnum} {RegionId}
    /// POS {RegionId} {SubId} {X} {Y} {Direction}
    /// ITEM {Id} {Vnum} {Chance}
    /// </summary>
    public class FishingResourceFileLoader : IResourceLoader<FishingSpotDto>
    {
        private const string FILE_NAME = "fish.dat";
        private readonly ResourceLoadingConfiguration _configuration;
        private readonly List<FishingSpotDto> _spots = new();
        private readonly List<FishingDropDto> _drops = new();

        // Map RegionIds to MapIds for spot creation
        private readonly Dictionary<int, int> _regionToMap = new();

        public FishingResourceFileLoader(ResourceLoadingConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IReadOnlyList<FishingDropDto> Drops => _drops;

        public async Task<IReadOnlyList<FishingSpotDto>> LoadAsync()
        {
            if (_spots.Count > 0)
            {
                return _spots;
            }

            string filePath = Path.Combine(_configuration.GameDataPath, FILE_NAME);

            if (!File.Exists(filePath))
            {
                Log.Warn($"[FISHING_LOADER] {filePath} not found. Skipping fishing data load.");
                return _spots;
            }

            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                await ParseFishingData(lines);
            }
            catch (Exception e)
            {
                Log.Error($"[FISHING_LOADER] Error loading {filePath}", e);
            }

            Log.Info($"[FISHING_LOADER] Loaded {_spots.Count} fishing spots and {_drops.Count} fish drops");
            return _spots;
        }

        private Task ParseFishingData(string[] lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#") || line.StartsWith("//")) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                try
                {
                    switch (parts[0])
                    {
                        case "MAP":
                            ParseMap(parts);
                            break;
                        case "POS":
                            ParsePosition(parts);
                            break;
                        case "ITEM":
                            ParseItem(parts);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"[FISHING_LOADER] Error parsing line: {line}. Error: {e.Message}");
                }
            }

            return Task.CompletedTask;
        }

        private void ParseMap(string[] parts)
        {
            // Format: MAP {MapVnum} {RegionId}
            if (parts.Length >= 3)
            {
                int mapVnum = int.Parse(parts[1]);
                int regionId = int.Parse(parts[2]);
                _regionToMap[regionId] = mapVnum;
            }
        }

        private void ParsePosition(string[] parts)
        {
            // Format: POS {RegionId} {SubId} {X} {Y} {Direction}
            if (parts.Length >= 6)
            {
                int regionId = int.Parse(parts[1]);
                // SubId at parts[2] - may be used for sub-zones
                short x = short.Parse(parts[3]);
                short y = short.Parse(parts[4]);
                short direction = short.Parse(parts[5]);

                // Get the map for this region
                if (!_regionToMap.TryGetValue(regionId, out int mapId))
                {
                    Log.Warn($"[FISHING_LOADER] No map defined for region {regionId}");
                    return;
                }

                _spots.Add(new FishingSpotDto
                {
                    MapId = mapId,
                    X = x,
                    Y = y,
                    Direction = direction,
                    RegionId = regionId,
                    MinLevel = 0
                });
            }
        }

        private void ParseItem(string[] parts)
        {
            // Format: ITEM {Id} {Vnum} {Chance} {RegionId} {MinLength} {MaxLength}
            if (parts.Length >= 4)
            {
                var drop = new FishingDropDto
                {
                    Id = int.Parse(parts[1]),
                    ItemVnum = int.Parse(parts[2]),
                    Chance = int.Parse(parts[3]),
                    RegionId = 0,
                    MinLevel = 0,
                    MaxLevel = 0,
                    MinFishLength = 10,
                    MaxFishLength = 100
                };

                if (parts.Length >= 5)
                {
                    drop.RegionId = int.Parse(parts[4]);
                }

                if (parts.Length >= 7)
                {
                    drop.MinFishLength = short.Parse(parts[5]);
                    drop.MaxFishLength = short.Parse(parts[6]);
                }

                _drops.Add(drop);
            }
        }
    }
}
