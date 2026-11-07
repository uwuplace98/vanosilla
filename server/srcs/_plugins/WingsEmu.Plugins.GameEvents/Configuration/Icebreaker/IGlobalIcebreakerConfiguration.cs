using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.GameEvent.Configuration;

namespace WingsEmu.Plugins.GameEvents.Configuration.Icebreaker
{
    public interface IGlobalIcebreakerConfiguration : IGlobalGameEventConfiguration
    {
        IcebreakerConfiguration GetInternalConfiguration(IPlayerEntity sessionCharacter);
    }
}
