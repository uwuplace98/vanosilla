using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugin.Fishing.Configuration;
using PhoenixLib.Logging;

namespace Plugin.Fishing
{
    public class FishingFileParser
    {
        private const string FishDatPath = "client-files/dat/fish.dat";

        public FishingConfiguration Parse()
        {
            var config = new FishingConfiguration();
            var path = Path.Combine(Environment.CurrentDirectory, FishDatPath);

            if (!File.Exists(path))
            {
                Log.Error($"[FishingFileParser] fish.dat not found at {path}");
                return config;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#") || line.StartsWith("//")) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var type = parts[0];

                try
                {
                    switch (type)
                    {
                        case "MAP":
                            // Format: MAP {MapVnum} {RegionId}
                            if (parts.Length >= 3)
                            {
                                config.Maps.Add(new FishingMap
                                {
                                    MapVnum = int.Parse(parts[1]),
                                    RegionId = int.Parse(parts[2])
                                });
                            }
                            break;

                        case "POS":
                            // Format: POS {RegionId} {SubId} {X} {Y} {Dir}
                            if (parts.Length >= 6)
                            {
                                config.Pools.Add(new FishingPool
                                {
                                    RegionId = int.Parse(parts[1]),
                                    SubId = int.Parse(parts[2]),
                                    X = int.Parse(parts[3]),
                                    Y = int.Parse(parts[4]),
                                    Direction = int.Parse(parts[5])
                                });
                            }
                            break;

                        case "ITEM":
                            // Format: ITEM {Id} {Vnum} {Chance} (Chance might be missing or complex)
                            // Based on header: #ITEM vnum chance?
                            // Let's assume ITEM {RegionId/Type?} {Vnum} {Chance}
                            // Need to verify further down in file.
                            // For now basic parsing:
                            if (parts.Length >= 4)
                            {
                                config.Drops.Add(new FishDrop
                                {
                                    Id = int.Parse(parts[1]),
                                    Vnum = int.Parse(parts[2]),
                                    Chance = int.Parse(parts[3])
                                });
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"[FishingFileParser] Error parsing line: {line}. Error: {e.Message}");
                }
            }

            Log.Info($"[FishingPlugin] Loaded {config.Maps.Count} maps, {config.Pools.Count} pools, {config.Drops.Count} drops.");
            return config;
        }
    }
}
