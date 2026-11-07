using Plugin.Minigames.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.Minigames
{
    public class MinigamesPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commandContainer;

        public MinigamesPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public string Name => nameof(MinigamesPlugin);

        public void OnLoad()
        {
            _commandContainer.AddModule<MinigamesCommandModule>();
        }
    }
}
