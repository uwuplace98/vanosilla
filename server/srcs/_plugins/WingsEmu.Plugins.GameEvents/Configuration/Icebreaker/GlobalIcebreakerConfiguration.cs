using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core; // For Range if needed, or System.Linq
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Maps;

namespace WingsEmu.Plugins.GameEvents.Configuration.Icebreaker
{
    public class GlobalIcebreakerConfiguration : IGlobalIcebreakerConfiguration
    {
        private readonly IcebreakerConfiguration _configuration;

        public GlobalIcebreakerConfiguration(IcebreakerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IGameEventConfiguration GetConfiguration(IPlayerEntity character)
        {
            var internalConfig = GetInternalConfiguration(character);
            if (internalConfig == null) return null;

            return new IcebreakerGameEventConfigurationAdapter
            {
                MapId = (short)internalConfig.MapId
            };
        }

        public IcebreakerInstanceConfiguration GetInternalConfiguration(IPlayerEntity character)
        {
            return _configuration.Channels
                .SelectMany(c => c.Instances)
                .FirstOrDefault(i => character.Level >= i.MinLevel && character.Level <= i.MaxLevel);
        }

        IcebreakerConfiguration IGlobalIcebreakerConfiguration.GetInternalConfiguration(IPlayerEntity sessionCharacter)
        {
            return _configuration;
        }

        public uint GetRegistrationCost(IPlayerEntity character)
        {
            return (uint)_configuration.JoinGoldCost;
        }
    }

    public class IcebreakerGameEventConfigurationAdapter : IGameEventConfiguration
    {
        public GameEventType GameEventType => GameEventType.Icebreaker;
        public short MapId { get; set; }
        public MapInstanceType MapInstanceType => MapInstanceType.EventGameInstance;
    }
}
