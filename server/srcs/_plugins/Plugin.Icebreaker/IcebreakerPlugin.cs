using Plugin.Icebreaker.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.Icebreaker
{
    public class IcebreakerPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commandContainer;

        public IcebreakerPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public string Name => nameof(IcebreakerPlugin);

        public void OnLoad()
        {
            _commandContainer.AddModule<IcebreakerCommandModule>();
        }
    }
}
