using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Plugin.Icebreaker.Configuration;
using WingsEmu.Game.Maps;

namespace Plugin.Icebreaker.Event.Icebreaker
{
    public enum IcebreakerState
    {
        Lobby,
        Battle,
        Finished
    }

    public class IcebreakerInstance
    {
        public IcebreakerInstance(IcebreakerInstanceConfiguration configuration, IMapInstance mapInstance)
        {
            Configuration = configuration;
            MapInstance = mapInstance;
        }

        public IcebreakerInstanceConfiguration Configuration { get; }
        public IMapInstance MapInstance { get; }

        // State
        public IcebreakerState State { get; set; } = IcebreakerState.Lobby;
        public DateTime StateStartTime { get; set; } = DateTime.UtcNow;
        public ConcurrentDictionary<long, byte> FrozenPlayers { get; } = new();
        public List<HashSet<long>> Teams { get; } = new();
        public object Lock { get; } = new();

        // Store original positions for return teleport (playerId -> (mapId, x, y))
        public ConcurrentDictionary<long, (Guid MapId, short X, short Y)> ReturnPositions { get; } = new();
    }
}
